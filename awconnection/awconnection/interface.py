"""
interface.py
=========================================
The main interface module of awconnection.
"""

import threading
import time
import json
import platform
import math

if platform.system() == "Windows":

    import win32pipe
    import win32file

from . import utility


class Vector3:

    """Basic encapsulation of of a 3D vector with several utility methods.

    Parameters
    ----------
    x : float
        The x component of the vector
    y : float
        The y component of the vector
    z : float
        The z component of the vector

    Attributes
    ----------
    x : float
        The x component of the vector
    y : float
        The y component of the vector
    z : float
        The z component of the vector
    """

    @classmethod
    def from_dict(cls, vector_dict):

        """Instantiates ``Vector3`` from dictionary of components.

        Parameters
        ----------
        vector_dict : dict of str: float
            Dictionary of components

        Returns
        -------
        Vector3
        """

        return cls(vector_dict["x"], vector_dict["y"], vector_dict["z"])

    @classmethod
    def zero(cls):

        """Instantiates a zero vector.

        Returns
        -------
        Vector3
            The zero vector
        """

        return Vector3(0, 0, 0)

    def __init__(self, x, y, z):
        self.x, self.y, self.z = x, y, z

    def __neg__(self):
        return Vector3(-self.x, -self.y, -self.z)

    def __add__(self, other):
        return Vector3(self.x + other.x, self.y + other.y, self.z + other.z)

    def __sub__(self, other):
        return Vector3(self.x - other.x, self.y - other.y, self.z - other.z)

    def __mul__(self, other):
        return Vector3(self.x * other, self.y * other, self.z * other)

    def __truediv__(self, other):
        return self * (1.0 / other)

    def magnitude(self):

        """Gets the magnitude of the vector.

        Returns
        -------
        float
            The magnitude of the vector
        """
        return math.sqrt(float(self.x ** 2.0 + self.y ** 2.0 + self.z ** 2.0))

    def normalized(self):

        """Returns the parallel vector with a magnitude of 1.

        Returns
        -------
        Vector3
            The normalized vector
        """
        magnitude = self.magnitude()
        return self / magnitude


class Altimeter(object):

    """Sensor determining the robot's altitude above the terrain.

    Attributes
    ----------
    altitude : float
        The robot's altitude above the terrain
    """

    def __init__(self, info_object_reference, info_dict):

        self.altitude = info_dict["altimeter"]["altitude"]
        self.__info_object_reference = info_object_reference


class GPS(object):

    """Sensor determining the robot's world position.

    Attributes
    ----------
    position : Vector3
        The robot's world position
    """

    def __init__(self, info_object_reference, info_dict):

        position_dict = info_dict["gps"]["position"]

        self.position = Vector3.from_dict(position_dict)
        self.velocity = Vector3.from_dict(info_dict["gps"]["velocity"])
        self.destination = Vector3.from_dict(info_dict["gps"]["destination"])
        self.__info_object_reference = info_object_reference


class Gyroscope(object):

    """Sensor determining the orientation of the robot.

    This class's three vector attributes—`forward`, `up`, and `right`—represent the robot's normalized local vectors
    in world coordinates. For example, if the robot was facing due north and straight upwards, `forward` would equal
    ``[0, 0, 1]``, `up` would equal ``[0, 1, 0]``, and `right` would equal ``[1, 0, 0]``. Euler angles and
    quaternions (possibly) coming soon.

    Attributes
    ----------
    forward : Vector3
        The normalized forward vector of the robot
    up : Vector3
        The normalized upwards vector of the robot
    right : Vector3
        The normalized rightwards vector of the robot
    """

    def __init__(self, info_object_reference, info_dict):

        gyroscope_dict = info_dict["gyroscope"]

        self.forward = Vector3.from_dict(gyroscope_dict["forward"])
        self.up = Vector3.from_dict(gyroscope_dict["up"])
        self.right = Vector3.from_dict(gyroscope_dict["right"])

        self.__info_object_reference = info_object_reference

        if self.__info_object_reference.coordinates_are_inverted:
            self.up *= -1
            self.right *= -1

        self.is_upside_down = gyroscope_dict["isUpsideDown"]


class Radar(object):

    """Sensor determining the locations of other robots.

    Attributes
    ----------
    pings : list of Vector3
        List of opponent locations in *relative, orientation-independent space*. To find the world location of the ping,
        add this ping vector to your world location.
    it_ping : Vector3
        Ping of "it" in classic tag. Evaluates to the zero vector if the robot is not playing classic tag.
    """

    def __init__(self, info_object_reference, info_dict):

        radar_dict = info_dict["radar"]

        self.pings = []

        for ping in radar_dict["pings"]:
            self.pings.append(Vector3.from_dict(ping))

        self.it_ping = Vector3.from_dict(radar_dict["itPing"])

        self.__info_object_reference = info_object_reference


class LiDAR(object):

    """Sensor determining the distance to obstacles in the world in the form of a spherical matrix of distances.

    Attributes
    ----------
    distance_matrix : list of list of float
        12x72 matrix of distances. This represents a 360 degree horizontal FOV, and 60 degree vertical FOV, from 30
        degrees below the robot to 30 degrees above. Both are currently at 5 degree resolution, with [0, 0] being
        straight ahead and 30 degrees above the robot, and [11, 71] being 355 degrees around the robot and 30
        degrees under it. Important note: everything is normal to the robot. If your robot is banked sideways,
        you will get a sideways view of the world. The only caveat is ``flip_coordinates()``—if you call
        ``flip_coordinates()``, left becomes right and up becomes down, just like everything else.
    """

    def __init__(self, info_object_reference, info_dict):

        self.__info_object_reference = info_object_reference

        container_array = info_dict["lidar"]["distanceMatrix"]
        """unity can't serialize 2D arrays (grr) so we have
        to create an array of ArrayContainers, which each
        each have a field "array" containing a row."""

        self.distance_matrix = []

        if self.__info_object_reference.coordinates_are_inverted:

            # flips up-down and left-right
            for container in container_array[::-1]:
                self.distance_matrix.append(container["array"][::-1])
        else:

            for container in container_array:
                self.distance_matrix.append(container["array"])


class TireDescription(object):

    def __init__(self, tire_dict):

        self.name = tire_dict["name"]
        self.motor_power = tire_dict["motorPower"]
        self.motor_torque = tire_dict["motorTorque"]
        self.brake_torque = tire_dict["brakeTorque"]
        self.rpm = tire_dict["rpm"]
        self.suspension_extension = tire_dict["suspensionExtension"]
        self.is_grounded = tire_dict["isGrounded"]
        self.spring_force = tire_dict["springForce"]
        self.longitudinal_slip = tire_dict["longitudinalSlip"]
        self.lateral_slip = tire_dict["lateralSlip"]


class Tires(object):

    def __init__(self, info_object_reference, info_dict):

        self.__info_object_reference = info_object_reference
        tire_dicts = info_dict["tires"]["tires"]

        self.__tires = dict()
        for tire_dict in tire_dicts:

            self.__tires[tire_dict["name"]] = TireDescription(tire_dict)

    def __getitem__(self, item):

        return self.__tires[item]

    def __setitem__(self, key, value):

        self.__tires[key] = value


class RobotInfo(object):

    """Class containing information about the robot

    Since this gets instantiated by ``RobotConnection.connect()``, there is no reason to ever instantiate this or any
    of its members. Instead, retrieve the robot's information using ``RobotConnection.get_info()``.

    Attributes
    ----------
    altimeter : Altimeter
        The robot's altimeter
    gps : GPS
        The robot's GPS
    gyroscope : Gyroscope
        The robot's gyroscope
    lidar : LiDAR
        The robot's LiDAR system
    radar : Radar
        The robot's Radar system
    timestamp : long
        The time in UTC-milliseconds that the sensors were last updated
    isIt : bool
        Indicates whether the robot is currently it. Evaluates to false if not playing tag.
    gamemode : {"Freeplay", "Classic Tag", "Singleplayer"}
        String containing the current gamemode
    """

    # I know this looks like bad programming, but until the two functions are actually different, there is no point
    # defining everything as None and then initializing it. If I just call update, python gets cranky.

    def __init__(self, info_dict):

        self.coordinates_are_inverted = False

        self.altimeter = Altimeter(self, info_dict)
        self.gps = GPS(self, info_dict)
        self.gyroscope = Gyroscope(self, info_dict)
        self.lidar = LiDAR(self, info_dict)
        self.radar = Radar(self, info_dict)
        self.tires = Tires(self, info_dict)

        self.timestamp = info_dict["timestamp"]
        self.is_it = info_dict["isIt"]
        self.gamemode = info_dict["gameMode"]
        self.map = info_dict["map"]
        self.has_game_started = info_dict["hasGameStarted"]


class RobotConnection:

    """Class providing an interface between python and AutonoWar robots.

    Attributes
    ----------
    info : RobotInfo
        This robot's current information
    """

    __SEND_INTERVAL = 0.01  # in seconds
    __GET_INTERVAL = 0.005
    __QUEUE_INTERVAL = 0.001

    __EVENT_SEPARATOR = ";"

    def __init__(self):

        if platform.system() == "Windows":
            self.__platform = "Windows"

        else:
            self.__platform = "POSIX"

        self.__event_buffer = []  # where events that need to be sent are stored
        self.__event_buffer_lock = threading.Lock()  # lock for threads wanting to access event_buffer

        self.info = None # type: RobotInfo
        self.__info_dict = None

        self.__INFO_PIPE_NAME = "RobotInfoPipe"
        self.__EVENT_QUEUE_PIPE_NAME = "EventQueuePipe"

        self.__info_coherent_lock = False  # allows user to prevent info from being updated
        self.__hidden_dict = None # where state is stored while info is locked
        self.__hidden_info = None

        self.__should_destroy = False  # should this connection end

        self.__connection_lock = threading.Condition()
        self.__send_connected = False
        self.__get_connected = False

    def __queue_event(self, event_text):

        with self.__event_buffer_lock:
            self.__event_buffer.append(event_text)

        time.sleep(RobotConnection.__QUEUE_INTERVAL)

    def flip_coordinates(self):

        """Rotates the internal coordinate system of the robot 180 degrees around the forward axis.

        This can be helpful if your robot flips over and you want to continue driving with old code. This reverses
        steering, torque, and lidar.

        Returns
        -------
        None
        """

        self.info.coordinates_are_inverted = not self.info.coordinates_are_inverted

        self.info.gyroscope = Gyroscope(self.info, self.__info_dict)  # flip coordinates immediately
        self.info.lidar = LiDAR(self.info, self.__info_dict)

    def set_tire_motor_power(self, tire_name, power):

        """Sets the power of tire motor `tire_name` to `power, where `power` is in watts.

        Parameters
        ----------
        tire_name : str
            The tire on which to apply the torque
        power : float
            Power, in watts

        Returns
        -------
        None
        """

        if self.info.coordinates_are_inverted:
            power *= -1

        self.__queue_event("SET tire power " + tire_name + " " + str(power))

    def set_tire_steering(self, tire_name, bearing):

        """Sets the steering of tire `tire_name` to `bearing`.

        Parameters
        ----------
        tire_name : str
            The tire on which to apply the torque
        bearing : float
            Bearing, in degrees clockwise off of vertical

        Returns
        -------
        None
        """
        if self.info.coordinates_are_inverted:
            bearing *= -1

        self.__queue_event("SET tire steering " + tire_name + " " + str(bearing))

    def set_tire_brake_torque(self, tire_name, torque):

        self.__queue_event("SET tire brake " + tire_name + " " + str(torque))

    def disconnect(self):

        """Ends this connection to AutonoWar.

        This is not strictly necessary if you are running your program in a ``while True`` loop, as long as you
        eventually stop the *entire* process, not just the main thread. A ``KeyboardInterrupt`` is enough;
        thread.exit() is not. If you fail to stop the process or call this method, the connection threads will
        continue hogging resources in the background.

        Returns
        -------
        None
        """

        self.__should_destroy = True

    def __send_buffer_thread_windows(self):

        event_queue_pipe = win32pipe.CreateNamedPipe(

            "\\\\.\\pipe\\" + self.__EVENT_QUEUE_PIPE_NAME,
            win32pipe.PIPE_ACCESS_DUPLEX,
            win32pipe.PIPE_TYPE_MESSAGE | win32pipe.PIPE_READMODE_BYTE | win32pipe.PIPE_WAIT,
            1, 65536, 65536,
            0,
            None

        )

        win32pipe.ConnectNamedPipe(event_queue_pipe, None)

        self.__connection_lock.acquire()
        self.__send_connected = True
        self.__connection_lock.notify_all()
        self.__connection_lock.release()

        while len(self.__event_buffer) > 0 or not self.__should_destroy:

            with self.__event_buffer_lock:

                message = self.__EVENT_SEPARATOR.join(self.__event_buffer) + "\n"
                self.__event_buffer = []

            if message != "\n":

                win32file.WriteFile(event_queue_pipe, bytes(message, "ascii"))

            time.sleep(self.__SEND_INTERVAL)

    def __send_buffer_thread_posix(self):

        fifo = utility.PosixFifo(self.__EVENT_QUEUE_PIPE_NAME, "w")

        self.__connection_lock.acquire()
        self.__send_connected = True
        self.__connection_lock.notify_all()
        self.__connection_lock.release()

        while len(self.__event_buffer) > 0 or not self.__should_destroy:

            with self.__event_buffer_lock:

                message = self.__EVENT_SEPARATOR.join(self.__event_buffer) + "\n"
                self.__event_buffer = []

            if message != "\n":

                fifo.stream.write(message)

            fifo.temp_close()
            time.sleep(self.__SEND_INTERVAL)
            fifo.re_init()

    def __get_robot_state_thread_windows(self):

        info_pipe = win32pipe.CreateNamedPipe(

            "\\\\.\\pipe\\" + self.__INFO_PIPE_NAME,
            win32pipe.PIPE_ACCESS_DUPLEX,
            win32pipe.PIPE_TYPE_MESSAGE | win32pipe.PIPE_READMODE_BYTE | win32pipe.PIPE_WAIT,
            1, 65536, 65536,
            0,
            None

        )

        win32pipe.ConnectNamedPipe(info_pipe, None)

        while not self.__should_destroy:

            info_text = utility.get_most_recent_message_windows(info_pipe)

            tmp_dict = json.loads(info_text)
            self.__info_dict = tmp_dict

            tmp_info = RobotInfo(self.__info_dict)

            if self.info:
                tmp_info.coordinates_are_inverted = self.info.coordinates_are_inverted

            self.info = tmp_info

            if not self.__get_connected:

                self.__connection_lock.acquire()
                self.__get_connected = True
                self.__connection_lock.notify_all()
                self.__connection_lock.release()

            time.sleep(self.__GET_INTERVAL)

    def lock_info(self):

        self.__info_coherent_lock = True

        self.__hidden_info = self.info
        self.__hidden_dict = self.__info_dict

    def unlock_info(self):

        self.info = self.__hidden_info
        self.__info_dict = self.__hidden_dict

        self.__info_coherent_lock = False

    def __get_robot_state_thread_posix(self):

        fifo = utility.PosixFifo(self.__INFO_PIPE_NAME, "r")

        while not self.__should_destroy:

            info_text = utility.get_most_recent_message_posix(fifo)

            if info_text != "":

                tmp_dict = json.loads(info_text)

                tmp_info = RobotInfo(tmp_dict)

                if self.info:

                    tmp_info.coordinates_are_inverted = self.info.coordinates_are_inverted

                if self.__info_coherent_lock:

                    self.__hidden_info = tmp_info
                    self.__hidden_dict = tmp_dict

                else:

                    self.info = tmp_info
                    self.__info_dict = tmp_dict

            if not self.__get_connected:

                self.__connection_lock.acquire()
                self.__get_connected = True
                self.__connection_lock.notify_all()
                self.__connection_lock.release()

            time.sleep(self.__GET_INTERVAL)

    def connect(self, pipe_index: int = 0):

        """Starts the connection to AutonoWar.

        Returns
        -------
        None
        """

        self.__connection_lock.acquire()
        self.__INFO_PIPE_NAME += str(pipe_index)
        self.__EVENT_QUEUE_PIPE_NAME += str(pipe_index)

        if self.__platform == "Windows":

            send_thread = threading.Thread(target=self.__send_buffer_thread_windows,)
            get_thread = threading.Thread(target=self.__get_robot_state_thread_windows)

        else:

            send_thread = threading.Thread(target=self.__send_buffer_thread_posix)
            get_thread = threading.Thread(target=self.__get_robot_state_thread_posix)

        send_thread.start()
        get_thread.start()

        while not self.__send_connected or not self.__get_connected:
            self.__connection_lock.wait()

        self.__connection_lock.release()
