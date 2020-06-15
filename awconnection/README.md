# AutonoWar Connection
An interface between Python and AutonoWar
<br>
<br>
Documentation (AutonoWar v0.1.3, abrconnection v0.1.0): <br><br>

* Coordinate system is left-handed, with x being east, y being upwards, and z being north.

* `RobotConnection()`: class which handles connection to the game. Should be instantiated at beginning of script, and method `connect()` should be called immediately after. `disconnect()` ends connection.  

* `RobotConnection.set_tire_torque(tire_name, torque)`: sets torque of tire `tire_name` to `torque`. Current tire names are "BackLeft", "BackRight", "FrontLeft", and "FrontRight."  

* `RobotConnection.set_tire_steering(tire_name, bering)`: sets tire `tire_name` to `bering`. All angles/berings are clockwise off of vertical (unity's coordinate system is left-handed).

* `RobotConnection.sensors`: Dictionary/Hashtable containing information about the state of the robot.  

  * Vectors are stored as dictionaries with keys `"x"`, `"y"`, and `"z"`.

  * `sensors["gps"]`: Sensor containing position information of the robot.
    
    * `sensors["gps"]["position"]`: Vector containing current position of robot relative to starting point.
   
  * `sensors["gyroscope"]`: Sensor containing rotation information of the robot:
  
    * `sensors["gyroscope"]["right"]`: Unit vector pointing right RELATIVE to the robot. For example, if the robot was facing in the default direction, its right vector would be <1, 0, 0> because its right direction is east. If the robot turned 90 degees counterclockwise, its right vector would be <0, 0, 1>. If the robot was facing a bering of 45 degrees and was climbing a 20 degree grade, its right vector would be <cos(45), sin(20), sin(45)> / sqrt(cos(45)^2 + sin(20)^2 + sin(45)^2).
    
    * `sensors["gyroscope"]["up"]`: Unit vector pointing up RELATIVE to the robot. Same idea as before.
    
    * `sensors["gyroscope"]["forward"]`: Unit vector pointing up RELATIVE to the robot. Same idea as before.
    
  * `sensors["lidar"]["distanceArray"]`: Array containing distance to any object at 1 degree increments. `state_dict["lidar"]["distanceArray"][0]` would describe how many meters of clearance the robot has in front of itself, `state_dict["lidar"]["distanceArray"][90]` would describe its clearance to the right, and so on. If the robot has more than 100 meters of clearance in a particular direction, the value will capped at 100. In future updates, lidar upgrades might include an increase in range or density for in-game currency. Vertical FOV will be coming soon.
  * `sensors["radar"]["pings"]`: array of vectors representing opponent locations
  * `sensors["altimeter"]["altitude"]`: distance to ground in world space (i.e. NOT normal to robot)

