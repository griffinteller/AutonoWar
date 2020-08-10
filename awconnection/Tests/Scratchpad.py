from awconnection import RobotConnection

r = RobotConnection()
r.connect()

power = 10000
steering = 0
brake = 0

r.set_tire_motor_power("1", power)
r.set_tire_motor_power("2", -power)
r.set_tire_motor_power("3", power)
r.set_tire_motor_power("4", -power)
r.set_tire_motor_power("mr", power)
r.set_tire_motor_power("ml", -power)

r.set_tire_steering("1", steering)
r.set_tire_steering("2", steering)

r.set_tire_brake_torque("1", brake)
r.set_tire_brake_torque("2", brake)
r.set_tire_brake_torque("3", brake)
r.set_tire_brake_torque("4", brake)
r.set_tire_brake_torque("mr", brake)
r.set_tire_brake_torque("ml", brake)

while True:

    print(r.info.tires["1"].longitudinal_slip)

r.disconnect()