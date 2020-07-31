import numpy as np
import matplotlib.pyplot as plt
import json


class FrictionCurve(object):

    class Integration(object):

        def __init__(self, curve, relative_torque_range, relative_torque_samples, asymptote_difference_threshold, slip_samples):

            self.curve = curve
            self.relativeTorques = np.linspace(relative_torque_range[0], relative_torque_range[1], relative_torque_samples)
            self.relativeTorqueStep = (relative_torque_range[1] - relative_torque_range[0]) \
                                      / (relative_torque_samples - 1)

            self.startingSlip = \
                np.sqrt(-np.log((asymptote_difference_threshold / (curve.y2 - curve.y3))) / np.log(2)) / curve.s + curve.x2

            self.slipStep = self.startingSlip / slip_samples
            self.slips = np.linspace(self.slipStep, self.startingSlip, slip_samples)
            self.matrix = np.zeros(shape=(relative_torque_samples, slip_samples))

            self.__generate_matrix()

        def plot_single_torque_bake(self, relative_torque_index):
            plt.plot(self.slips, self.matrix[relative_torque_index], 'r')

        def __generate_matrix(self):

            self.matrix[:, self.matrix.shape[1] - 1] = -self.slipStep / (-self.curve.y3 + self.relativeTorques)
            for step in range(self.matrix.shape[1] - 1):
                slip = self.startingSlip - step * self.slipStep
                self.matrix[:, self.matrix.shape[1] - step - 2] = self.__get_delta_t(slip, self.relativeTorques)

        def __get_delta_t(self, slip, relative_torque):
            coef = self.curve.c(slip)
            accel = -coef + relative_torque
            return -self.slipStep / accel

    class Encoder(json.JSONEncoder):

        def default(self, o):

            if isinstance(o, FrictionCurve):

                dct = o.__dict__
                return dct

            elif isinstance(o, FrictionCurve.Integration):

                dct = o.__dict__
                del dct["curve"]
                return dct

            elif isinstance(o, np.ndarray):

                return o.tolist()

            else:
                return json.JSONEncoder.default(self, o)

    def __init__(self, x1=0.5, y1=0.95, x2=1.5, y2=1.1, x3=3, y3=0.7, stiffness=1):

        self.x1 = x1
        self.y1 = y1
        self.x2 = x2
        self.y2 = y2
        self.x3 = x3
        self.y3 = y3
        self.s = np.sqrt(1 / (2 * np.log(2) * (x3 - x2) ** 2))
        self.stiffness = stiffness
        self.integration = FrictionCurve.Integration(self, (-2*y2, 2*y2), 50, 0.01, 1000)

    def c1(self, slip):
        return (self.y1 / self.x1) * slip

    def c2(self, slip):
        ix = self.x1 * self.y2 / self.y1
        a = self.x1 - 2 * ix + self.x2

        return (self.y1 - self.y2) * ((self.x1 - ix + np.sqrt((self.x1 - ix) ** 2 - (self.x1 - slip) * a)) / a - 1) ** 2 + self.y2

    def c3(self, slip):
        return (self.y2 - self.y3) * (2 ** (-((self.s * (slip - self.x2)) ** 2))) + self.y3

    def c(self, slip):

        if slip < self.x1:
            coefficient = self.c1(slip)
        elif slip < self.x2:
            coefficient = self.c2(slip)
        else:
            coefficient = self.c3(slip)

        return coefficient * self.stiffness

    def c_vec(self, slips):

        result = np.empty(shape=slips.shape)
        for i in range(slips.shape[0]):

            result[i] = self.c(slips[i])

        return result

    def save_to_file(self,  filename):

        with open(filename, "w") as f:
            f.write(FrictionCurve.Encoder().encode(self))

    def plot_curve(self, x_max, samples=100):

        x = np.linspace(0, x_max, samples)
        y = self.c_vec(x)
        plt.plot(x, y, 'r')

curve = FrictionCurve()
curve.plot_curve(5)
plt.show()

curve.save_to_file("DefaultCurve.fcurve")
