from awconnection import RobotConnection

r = RobotConnection()
r.connect()

r.set_tire_torque("1", 1000)
r.set_tire_torque("2", -1000)

r.disconnect()