import numpy as np
import matplotlib.pyplot as plt

from scipy.integrate import quad

# Bakes information that is relevent to finding out how much a tire's slip should change over the course of a frame
# depending on torque and friction. Everything but torque changes this value linearly, so we don't need to bake this in.
# We just solve the differential equation across a given set of relative torque values, and a range of slip values.
# At runtime, we load the nearest two baked slip values and interpolate to get our approximate solution.

# slip is in m/s

x1 = 0.5  # adherent slip max
y1 = 0.95  # adherent friction max
x2 = 1.5  # peak slip
y2 = 1.1  # peak friction
x3 = 3  # tail inflection x
y3 = 0.1  # asymptote
stiffness = 1  # multiplier for friction coefs. Quick way to change the feeling of surface.

s = np.sqrt(1 / (2 * np.log(2) * (x3 - x2) ** 2))

# relative torque is torque / (down_force * radius). Calculating with this value allows baked info to be used on all
# tires in all situations. The only time friction needs to be re-baked is if the curve changes. For the purposes of this
# calculation, the tire has a moment of inertia (I) of 1 kg m2, a down force (S) of 1 N, and a radius (r) of 1.

# A good range is [-2 * y2, 2 * y2]
relative_torque_range = (-2 * y2, 2 * y2)
relative_torque_step = 0.1

asymptote_difference_threshold = 0.01  # when the curve is this much above the asymptote, we start integration
slip_step = 0.02

# internal
starting_x = np.sqrt(-np.log((asymptote_difference_threshold / (y2 - y3))) / np.log(2)) / s + x2
num_slip_steps = int(np.ceil(starting_x / slip_step))
starting_x = num_slip_steps * slip_step


def c1(slip):
    return (y1 / x1) * slip


def c2(slip):
    ix = x1 * y2 / y1
    a = x1 - 2 * ix + x2

    return (y1 - y2) * ((x1 - ix + np.sqrt((x1 - ix) ** 2 - (x1 - slip) * a)) / a - 1) ** 2 + y2


def c3(slip):
    return (y2 - y3) * (2 ** (-((s * (slip - x2)) ** 2))) + y3


def c(slip):
    coefficient = 0

    if slip < x1:
        coefficient = c1(slip)
    elif slip < x2:
        coefficient = c2(slip)
    else:
        coefficient = c3(slip)

    return coefficient * stiffness


def c_vec(slips: np.ndarray):
    result = np.zeros(shape=slips.shape)
    for i in range(slips.shape[0]):
        result[i] = c(slips[i])

    return result


def plot_c(x_min, x_max):
    x = np.linspace(x_min, x_max, 100)
    y = c_vec(x)
    plt.plot(x, y, 'r')


def plot_c_with_lines(x_min, x_max):
    plot_c(0, 8)
    plt.vlines(starting_x, 0, y2)
    plt.hlines(y3, 0, 8)


def get_delta_t(slip, relative_torque):
    coef = c(slip)
    accel = -coef + relative_torque
    return -slip_step / accel


def integrate_time_slices(relative_torque):
    # we put one at the end for the asymptote. At values beyond starting_x, we just take the final value
    result = np.zeros(shape=(num_slip_steps + 1,))

    result[num_slip_steps] = -slip_step / (-y3 + relative_torque)

    for step in range(num_slip_steps):
        slip = starting_x - step * slip_step
        result[num_slip_steps - step - 1] = get_delta_t(slip, relative_torque)

    return result


def plot_single_torque_bake(relative_torque):
    x = np.linspace(slip_step, (num_slip_steps + 1) * slip_step, num_slip_steps + 1)
    y = integrate_time_slices(relative_torque)
    plt.scatter(x, y)


def integrate_across_relative_torques(torques: np.ndarray =
                                      np.linspace(relative_torque_range[0], relative_torque_range[1],
                                                  int((relative_torque_range[1] - relative_torque_range[0])
                                                      / relative_torque_step + 1))):

    result = np.zeros(shape=(torques.shape[0], num_slip_steps + 1))
    result[:, num_slip_steps] = -slip_step / (-y3 + torques)
    for step in range(num_slip_steps):
        slip = starting_x - step * slip_step
        result[:, num_slip_steps - step - 1] = get_delta_t(slip, torques)

    return result


def plot_bake():

    bake = integrate_across_relative_torques(np.array())
    for single in bake:

        x = np.linspace(slip_step, (num_slip_steps + 1) * slip_step, num_slip_steps + 1)
        plt.plot(x, single, 'r')

plot_single_torque_bake(0.5)
plt.show()

