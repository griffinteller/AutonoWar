import numpy as np
import matplotlib.pyplot as plt
from scipy.integrate import dblquad
from pprint import pprint

"""

Plan: 
-Get test points (in x and y, 2d is fine)
-Get Integrand (1 over d^2 times unit vector)

"""

R = 1000  # meters
r = 200
G = 6.67430e-11
D = 10000  # kg/m^3
offset = 1


def integrand(phi: float, theta: float, x: float, y: float, z: float, idx: int):
    s = np.array(
        (np.cos(phi) * (R + r * np.cos(theta)) - x,
         r * np.sin(theta) - y,
         -1 * np.sin(phi) * (R + r * np.cos(theta)) - z))

    return (s / (np.linalg.norm(s) ** 3) * (r * (R + r * np.cos(theta))))[idx]


def get_accel_at_point(pt: np.ndarray):

    accel = np.empty(shape=(3,))
    accel[0] = dblquad(integrand, 0, 2 * np.pi,
                       lambda theta: 0, lambda theta: 2 * np.pi, (pt[0], pt[1], pt[2], 0))[0]
    accel[1] = dblquad(integrand, 0, 2 * np.pi,
                       lambda theta: 0, lambda theta: 2 * np.pi, (pt[0], pt[1], pt[2], 1))[0]
    accel[2] = dblquad(integrand, 0, 2 * np.pi,
                       lambda theta: 0, lambda theta: 2 * np.pi, (pt[0], pt[1], pt[2], 2))[0]

    return accel


def get_test_points(rows, cols, max_row_rad):
    arr = np.empty(shape=(rows, cols, 3))

    for row in range(rows):
        for col in range(cols):
            rad = offset + r + (max_row_rad - r) * (row / (rows - 1))
            arr[row][col][0] = R + rad * np.cos(col / (cols - 1) * np.pi)
            arr[row][col][1] = rad * np.sin(col / (cols - 1) * np.pi)
            arr[row][col][2] = 0

    return arr


def get_accels_at_test_points(test_points: np.ndarray):
    shp = test_points.shape
    ans = np.empty(shape=shp)
    for row in range(shp[0]):
        for col in range(shp[1]):
            print(row, col)
            accel = get_accel_at_point(test_points[row][col])
            ans[row][col] = accel

    return ans


test_points = get_test_points(6, 21, 600)
print(test_points)
accels = get_accels_at_test_points(test_points)

flt_test_points = np.reshape(test_points, (test_points.shape[0] * test_points.shape[1], 3))
x, y = flt_test_points[:, 0], flt_test_points[:, 1]
print(x)

flt_accels = np.reshape(accels, (accels.shape[0] * accels.shape[1], 3))
u = flt_accels[:, 0]
v = flt_accels[:, 1]

plt.quiver(x, y, u, v)
plt.show()
