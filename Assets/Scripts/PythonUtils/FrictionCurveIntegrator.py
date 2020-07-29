import numpy as np
import matplotlib.pyplot as plt

from scipy.integrate import quad
from math import sqrt, log

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
y3 = 0.8  # asymptote
stiffness = 1  # multiplier for friction coefs. Quick way to change the feeling of surface.

s = sqrt(1/(2*log(2)*(x3-x2)**2))


# relative torque is torque / (down_force * radius). Calculating with this value allows baked info to be used on all
# tires in all situations. The only time friction needs to be re-baked is if the curve changes.
#
# A good range is [-2 * y2, 2 * y2]

relative_torque_range = (-2 * y2, 2 * y2)
relative_torque_step = 0.1

asymptote_difference_threshold = 0.05  # when the curve is this much above the asymptote, we start integration
slip_step = 0.05


def c1(slip):
    return (y1 / x1) * slip


def c2(slip):

    ix = x1 * y2 / y1
    a = x1 - 2*ix + x2

    return (y1 - y2) * ((x1 - ix + sqrt((x1 - ix)**2 - (x1 - slip) * a)) / a - 1)**2 +y2


def c3(slip):
    return (y2 - y3) * (2 ** (-((s * (slip - x2))**2))) + y3


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
    plt.show()


plot_c(0, 5)
