# autonomous-battle-royale
Battle royale with autonomous, user-programmed players

GitHub is being uncooperative with large files, so until I beat it into submission, I am just going to keep links to builds here:<br><br>
Mac 0.0.3: https://developer.cloud.unity3d.com/share/share.html?shareId=ZJQqY1oAoB<br>
Windows 0.0.3: https://developer.cloud.unity3d.com/share/share.html?shareId=Wk_UsksRir<br><br>
Mac 0.0.2: https://developer.cloud.unity3d.com/share/share.html?shareId=-10PBxgAoB<br>
<br>
<br>
Documentation (0.0.3): <br><br>

* Coordinate system is left-handed, with x being east, y being upwards, and z being north.

* `RobotConnection()`: class which handles connection to the game. Should be instantiated at beginning of script, and method `connect()` should be called immediately after. `disconnect()` ends connection.  

* `RobotConnection.set_tire_torque(tire_name, torque)`: sets torque of tire `tire_name` to `torque`. Current tire names are "BackLeft", "BackRight", "FrontLeft", and "FrontRight."  

* `RobotConnection.set_tire_steering(tire_name, bering)`: sets tire `tire_name` to `bering`. All angles/berings are clockwise off of vertical (unity's coordinate system is left-handed).

* `RobotConnection.state_dict`: Dictionary/Hashtable containing information about the state of the robot.  

  * Vectors are stored as dictionaries with keys `"x"`, `"y"`, and `"z"`.

  * `state_dict["gps"]`: Sensor containing position information of the robot.
    
    * `state_dict["gps"]["position"]`: Vector containing current position of robot relative to starting point.
   
  * `state_dict["gyroscope"]`: Sensor containing rotation information of the robot:
  
    * `state_dict["gyroscope"]["right"]`: Unit vector pointing right RELATIVE to the robot. For example, if the robot was facing in the default direction, its right vector would be <1, 0, 0> because its right direction is east. If the robot turned 90 degees counterclockwise, its right vector would be <0, 0, 1>. If the robot was facing a bering of 45 degrees and was climbing a 20 degree grade, its right vector would be <cos(45), sin(20), sin(45)> / sqrt(cos(45)^2 + sin(20)^2 + sin(45)^2).
    
    * `state_dict["gyroscope"]["up"]`: Unit vector pointing up RELATIVE to the robot. Same idea as before.
    
    * `state_dict["gyroscope"]["forward"]`: Unit vector pointing up RELATIVE to the robot. Same idea as before.
    
  * `state_dict["lidar"]`: Array containing distance to any object at 1 degree increments. `state_dict["lidar"][0]` would describe how many meters of clearance the robot has in front of itself, `state_dict["lidar"][90]` would describe its clearance to the right, and so on. If the robot has more than 100 meters of clearance in a particular direction, the value will capped at 100. In future updates, lidar upgrades might include an increase in range or density for in-game currency. Vertical FOV will be coming soon.
