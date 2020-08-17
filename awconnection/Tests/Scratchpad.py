from awconnection import RobotConnection
import time

r = RobotConnection()
r.connect()

power = 30000
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

    r.lock_info()

    start_time = time.time()
    start_x = r.info.gps.position.x
    while time.time() - start_time < 10:

        new_x = r.info.gps.position.x
        if new_x != start_x:

            print("WRONG! x was " + str(start_x) + "but now is " + str(new_x))

    r.unlock_info()

r.disconnect()