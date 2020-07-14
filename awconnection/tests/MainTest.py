from awconnection import RobotConnection
import matplotlib.pyplot as plt
import time

r = RobotConnection()
r.connect(3)


def set_steering(bearing):

    r.set_tire_steering("1", bearing)
    r.set_tire_steering("2", bearing)


def set__torque(torque):

    r.set_tire_torque("1", torque)
    r.set_tire_torque("2", -torque)

while True:

    r.lock_info()

    set__torque(1000)

    r.unlock_info()
    r.disconnect()
