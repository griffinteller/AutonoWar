from awconnection import RobotConnection
import time

r = RobotConnection()
r.connect(0)


def set_steering(bearing):

    r.set_tire_steering("1", bearing)
    r.set_tire_steering("2", bearing)


def set__torque(torque):

    r.set_tire_torque("1", torque)
    r.set_tire_torque("2", -torque)

set__torque(2000)
while True:

    r.lock_info()
    print(r.info.has_game_started)
    r.unlock_info()
    time.sleep(0.05)

r.disconnect()
