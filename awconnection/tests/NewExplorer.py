from awconnection import RobotConnection
import time, math
import scipy as sp
import numpy as np
import datetime
import tkinter as tk
import time, math

robot = RobotConnection()
robot.connect()

# These 4 must be the same in Dashboard!!
x_map_reduce = 50
y_map_reduce = 50
half_map_width = 100
half_map_height = 100



savetime = int(round(time.time()))

base_speed = 1  # Use 2.2 for tag, 1.8 for Fight, 1.2 for Explore

current_bearing = 0.1
current_speed = 0.1
original_goal_direction = 0.1
last_speed = 0.1

any_fails = False

is_in_canyon = False

pos = robot.info.gps.position
last_gps = np.array([pos.x, pos.y, pos.z])
start_maps_over = False

last_max_speed = 0

Flipped = False
Pirouetting = False

then = int(round(time.time() * 1000))

# up = bot.info.gyroscope.up
# print(up.x)
# print(up.y)
# print(up.z)
# bot.disconnect()

# THINGS TO DO

# Fix charge enemy and flee enemy so that they take a robot.dict instead of an index

# Get Lidar helping in bias_off_roll_and_clear_space

# Get Lidar helping to see if it is bumpy

# Get Lidar helping to slow down if things are dangerous

# Start a learning systetm to optimize characteristics to minimize flipping

# set up so flipping isn't a problem for pirouetting

# SAVE!!

current_torque = 100
goal_direction = np.random.random() * 2 * 3.14159 - 3.14159  # in radians

steer_radians = 0  # in radians
max_steer = .32  # in radians
debug = False

last_speed = 0
speed_burst = 1.4
max_torque = 2400

then = time.time()
last_slope = 0

IT = robot.info.isIt

last_alt = [.4, .4, .4, .4, .4]
avg_alt = .41

robot_gps = robot.info.gps.position

initial_gps = np.array([robot_gps.x, robot_gps.y, robot_gps.z])
print("Initial GPS = ", initial_gps)

if debug:
    print("Initial GPS = ", initial_gps)
last_gps = initial_gps
if debug:
    print("Last GPS =", last_gps)


# ------------------------------------------
def setup_maps(new_world):
    global elevation_high_map
    global elevation_low_map
    global avg_speed_map
    global num_fails_map
    global num_visits_map
    global clear_space_map

    if new_world:

        elevation_high_map = np.array([[0 for i in range(half_map_width * 2)] for j in range(half_map_height * 2)])
        elevation_low_map = np.array([[10000 for i in range(half_map_width * 2)] for j in range(half_map_height * 2)])
        avg_speed_map = np.array([[0 for i in range(half_map_width * 2)] for j in range(half_map_height * 2)])
        num_fails_map = np.array([[0 for i in range(half_map_width * 2)] for j in range(half_map_height * 2)])
        num_visits_map = np.array([[0 for i in range(half_map_width * 2)] for j in range(half_map_height * 2)])
        clear_space_map = np.array([[0 for i in range(half_map_width * 2)] for j in range(half_map_height * 2)])

        np.savetxt("elevation_high_map.txt", elevation_high_map)
        np.savetxt("elevation_low_map.txt", elevation_low_map)
        np.savetxt("avg_speed_map.txt", avg_speed_map)
        np.savetxt("num_fails_map.txt", num_fails_map)
        np.savetxt("num_visits_map.txt", num_visits_map)
        np.savetxt("clear_space_map.txt", clear_space_map)

    else:

        elevation_high_map = np.loadtxt("elevation_high_map.txt")
        elevation_low_map = np.loadtxt("elevation_low_map.txt")
        avg_speed_map = np.loadtxt("avg_speed_map.txt")
        num_fails_map = np.loadtxt("num_fails_map.txt")
        num_visits_map = np.loadtxt("num_visits_map.txt")
        clear_space_map = np.loadtxt("clear_space_map.txt")

    # ------------------------------------------


def check_radians(bearing):
    if bearing < -math.pi or bearing > math.pi:
        print(" ***   ###   AHHH!!!  Radians Broken   ###   ***   ")


# -------------------------------------
def behind_me(enemy_bearing):
    return enemy_bearing < -math.pi / 2 or enemy_bearing > math.pi / 2


# -------------------------------------
def head_nearly_directly_away(enemy_bearing):
    if enemy_bearing <= 0:
        goal_direction = enemy_bearing + math.pi * (1 - np.random.random() * 0.03)
    else:
        goal_direction = enemy_bearing - math.pi * (1 - np.random.random() * 0.03)

    check_radians(goal_direction)
    return goal_direction


# -------------------------------------
def on_a_serious_up_slope():
    global last_slope

    robot_vector = get_bearing_vec(robot.info.gyroscope.forward)

    if last_slope > 0.3 and robot_vector[1] > last_slope:
        last_slope = robot_vector[1]
        return True
    else:
        last_slope = robot_vector[1]
        return False


# -------------------------------------
def slope():
    robot_vector = get_bearing_vec(robot.info.gyroscope.forward)

    return robot_vector[1]


# -------------------------------------
def rad_safe_add(goal_dir, change):
    goal_dir = goal_dir + change
    if goal_dir > math.pi:
        goal_dir -= 2 * math.pi
    elif goal_dir < -math.pi:
        goal_dir += 2 * math.pi

    check_radians(goal_dir)
    return goal_dir


# -------------------------------------
def turn_on_slope_avoid_enemy(enemy_bearing):
    global goal_direction

    if enemy_bearing <= 0:
        goal_direction = rad_safe_add(goal_direction, math.pi / 4)
    else:
        goal_direction = rad_safe_add(goal_direction, -math.pi / 4)

    check_radians(goal_direction)
    return goal_direction


# -------------------------------------
def flee_enemy(enemy):
    global goal_direction

    enemy_bearing = get_enemy_bearing(enemy)

    if behind_me(enemy_bearing):
        goal_direction = bias_off_roll_and_clear_space(goal_direction)
    else:
        goal_direction = head_nearly_directly_away(enemy_bearing)

    check_radians(goal_direction)
    return goal_direction


# -------------------------------------
def get_dist_to_foe(enemy):
    enemy_arr = np.array([enemy.x, enemy.y, enemy.z])
    return vec_dist(enemy_arr, [0, 0, 0])


# -------------------------------------
def get_enemy_bearing(enemy):
    altitudeless_bearing_vector = np.array([enemy.x, 0, enemy.z])
    size = np.linalg.norm(altitudeless_bearing_vector)
    altitudeless_bearing_vector /= size

    enemy_bearing = math.acos(altitudeless_bearing_vector[2])
    if enemy.x < 0:
        enemy_bearing = -enemy_bearing

    check_radians(enemy_bearing)
    return enemy_bearing


# -------------------------------------
def charge_enemy(enemy):
    global goal_direction

    enemy_bearing = get_enemy_bearing(enemy)

    goal_direction = enemy_bearing

    return goal_direction


# -------------------------------------
def set_torque(level):
    robot.set_tire_torque("BackLeft", level)
    robot.set_tire_torque("BackRight", level)
    robot.set_tire_torque("FrontLeft", level)
    robot.set_tire_torque("FrontRight", level)


# -------------------------------------
def steer(amount):
    global steer_radians
    global max_steer

    if (amount < 0 and steer_radians > 0) or (amount > 0 and steer_radians < 0):
        amount = 0

    steer_radians = amount
    if steer_radians > max_steer:
        steer_radians = max_steer
    elif steer_radians < -max_steer:
        steer_radians = -max_steer

    if abs(steer_radians) < 0.03:
        steer_radians = 0

    robot.set_tire_steering("FrontLeft", steer_radians * 57.3)  # convert to angles
    robot.set_tire_steering("FrontRight", steer_radians * 57.3)  # convert to angles
    robot.set_tire_steering("BackLeft", 0)  # convert to angles
    robot.set_tire_steering("BackRight", 0)  # convert to angles


# -------------------------------------
def might_print(percent_chance, string, num):
    if np.random.random() < (percent_chance / 100):
        print(string, num)
        return True
    else:
        return False


# -------------------------------------
def jump():
    robot.set_tire_torque("BackLeft", 0)
    robot.set_tire_torque("BackRight", 0)
    robot.set_tire_torque("FrontLeft", -max_torque * 10)
    robot.set_tire_torque("FrontRight", -max_torque * 10)
    print("jumping!!")


# -------------------------------------
def enemy_very_close(index, cur_speed):
    enemies = robot.info.radar.pings

    if len(enemies) > index:
        enemy_dist = get_dist_to_foe(enemies[index])
        if enemy_dist < 2 * cur_speed:  # This assume cur_speed is distance / second, so within a few seconds of touching robot
            return True

    return False


# -------------------------------------
def roll():
    global robot

    robot_vector = get_bearing_vec(robot.info.gyroscope.right)

    roll = math.asin(robot_vector[1])

    if robot.info.gyroscope.is_upside_down:
        roll = - roll

    return roll


# -------------------------------------
def distance_between_angles(ang1, ang2):  # in radians

    if ang1 + math.pi < ang2:
        ang1 += 2 * math.pi
    elif ang2 + math.pi < ang1:
        ang2 += 2 * math.pi

    return ang2 - ang1


# -------------------------------------
def slope_adjusted_breaking(cur_slope, cur_speed, max_speed):
    default_breaking = -max_torque / 40

    scale_up_breaking = min((cur_speed / max_speed) * (cur_speed / 4), 10)  # assumes cur_speed > max_speed

    slope_adjustment = cur_slope / 0.5  # 0.3 is a strong up or down slope so this will be 1 at slope of 0.5 and -1 at slope of -0.5

    return default_breaking * scale_up_breaking * (
                1 - slope_adjustment)  # it is possible that on a VERY steep up slope the "breaking" will still be forward!


# -------------------------------------
def keep_speed(cur_speed, cur_bearing, target_bearing, margin):
    global goal_direction
    global current_torque
    global last_speed
    global steer_radians
    global last_max_speed
    global any_fails

    max_speed = base_speed

    c_n_bumpy = check_not_bumpy()
    if c_n_bumpy:
        max_speed *= 1.5

    if abs(distance_between_angles(cur_bearing, target_bearing)) < 1.5 * margin:
        max_speed *= 1.5
        if abs(distance_between_angles(cur_bearing, target_bearing)) < 0.75 * margin:
            max_speed *= 1.5  # total of two of not heavily steering

    cur_slope = slope()
    if abs(cur_slope) < .20:
        max_speed *= 1.5
    else:
        might_print(1, "Slope high = ", cur_slope)

    cur_roll = roll()
    if abs(cur_roll) < .20:
        max_speed *= 1.5
    else:
        might_print(1, "Roll high = ", cur_roll)

    if enemy_very_close(0, cur_speed):
        max_speed *= speed_burst

    if max_speed > last_max_speed:
        max_speed = (max_speed + last_max_speed) / 2
    else:
        max_speed = (2 * max_speed + last_max_speed) / 3

    might_print(1, "Max Speed = ", max_speed)
    last_max_speed = max_speed

    if current_torque >= max_torque and ((cur_speed < (max_speed / 50) and cur_speed < last_speed) or cur_slope > 0.85):
        print("Giving Up and Going Back! with slope = ", cur_slope)
        any_fails = True
        current_torque = -max_torque / 4
    elif cur_speed < max_speed:
        if cur_speed + (cur_speed - last_speed) * 8 < max_speed:
            current_torque = min(current_torque + max_torque * 0.20, max_torque)
        else:
            current_torque = min(current_torque + max_torque * 0.1, max_torque)
    else:
        current_torque = slope_adjusted_breaking(cur_slope, cur_speed, max_speed)
        if np.random.random() < 0.01:
            print("Breaking ",
                  current_torque)  # max_speed,cur_speed,c_n_bumpy,abs(distance_between_angles(cur_bearing,target_bearing)),cur_slope,cur_roll)

    set_torque(current_torque)

    last_speed = cur_speed


# -------------------------------------
def pick_unvisited_area():
    x = int(round(np.random.random() * (2 * half_map_width - 1)))
    y = int(round(np.random.random() * (2 * half_map_height - 1)))

    while num_visits_map[x][y] > 0:
        x = int(round(np.random.random() * (2 * half_map_width - 1)))
        y = int(round(np.random.random() * (2 * half_map_height - 1)))

    return x, y


# -------------------------------------
def passability_of_area(x, y):
    global elevation_high_map
    global elevation_low_map
    global avg_speed_map
    global num_fails_map
    global num_visits_map
    global clear_space_map

    if (x < 0 or x >= 2 * half_map_width) or (y < 0 or y >= 2 * half_map_height):
        print("Error in  Passability!")

    num_visits = num_visits_map[x][y]
    if num_visits == 0:
        return 0

    num_clear_spaces = clear_space_map[x][y]
    max_el = elevation_high_map[x][y]
    min_el = elevation_low_map[x][y]
    num_fails = num_fails_map[x][y]
    avg_speed = avg_speed_map[x][y]

    # CLEAN UP NEEDED!
    return avg_speed + math.log(1 + num_visits) + 10 * (num_clear_spaces / num_visits) - math.log(
        1 + max_el - min_el) - 1000 * (num_fails / num_visits)


# -------------------------------------
def vec_dist(vec1, vec2):
    return (((vec1[0] - vec2[0]) ** 2) + ((vec1[1] - vec2[1]) ** 2) + ((vec1[2] - vec2[2]) ** 2)) ** 0.5


# -------------------------------------
def steer_control(current_speed, cur_bearing, target_bearing, margin):
    global steer_radians
    global max_steer

    if abs(cur_bearing - target_bearing) > math.pi * 1.01:  # i.e. if it is shorter to go the other way
        if target_bearing < 0:
            target_bearing += 2 * math.pi  # Not rad safe right now but it works
        else:
            target_bearing -= 2 * math.pi  # Not rad safe right now but it works

    if current_speed > 9:
        max_steer_now = max_steer / 3
    elif current_speed > 5:
        max_steer_now = max_steer / 2
    else:
        max_steer_now = max_steer

    if cur_bearing - target_bearing > 0:
        steer(- max_steer_now * min((cur_bearing - target_bearing) / margin, 1.0) ** 2)

    elif cur_bearing - target_bearing < 0:
        steer(max_steer_now * max((cur_bearing - target_bearing) / margin, -1.0) ** 2)
    else:
        steer(0)


# -------------------------------------
def get_hillless_bearing(robot_vec):
    vector = np.array([robot_vec[0], 0, robot_vec[2]])
    size = np.linalg.norm(vector)
    vector /= size

    bearing = math.acos(vector[2])
    if vector[0] < 0:
        bearing = -bearing

    return bearing


# -------------------------------------
def get_bearing(robot_vec):
    vector = np.array([robot_vec[0], robot_vec[1], robot_vec[2]])
    size = np.linalg.norm(vector)
    vector /= size

    bearing = math.acos(vector[2])
    if vector[0] < 0:
        bearing = -bearing

    return bearing


# -------------------------------------
def get_bearing_vec(robot_list):
    robot_vector = np.array([robot_list.x, robot_list.y, robot_list.z])

    size = np.linalg.norm(robot_vector)
    robot_vector /= size

    return robot_vector


# -------------------------------------
def get_canyon_lidar():
    global robot

    cur_lidar = np.array(robot.info.lidar.distance_matrix)
    cur_canyon_lidar = np.array(cur_lidar[:7])

    return cur_canyon_lidar


# -------------------------------------
def get_front_high_lidar():
    global robot

    cur_lidar = np.array(robot.info.lidar.distance_matrix)
    cur_lidar_right = np.array(cur_lidar[:10, :6])
    cur_lidar_left = np.array(cur_lidar[:10, -6:])
    cur_lidar_front = np.concatenate((cur_lidar_left, cur_lidar_right), 1)

    return cur_lidar_front


# -------------------------------------
def get_lidar_std(lidar, row):  # lidar is 2D

    temp_arr = np.array(lidar[row])

    return np.std(temp_arr)


# -------------------------------------
def get_lidar_average(lidar, row):
    return sum(lidar[row]) / len(lidar[row])


# -------------------------------------
def check_not_bumpy():
    global last_alt
    global avg_alt

    alt = robot.info.altimeter.altitude

    lidar = np.array(get_front_high_lidar())

    last_alt = last_alt[1:len(last_alt)]
    last_alt.append(alt)

    max_alt = max(last_alt)
    min_alt = min(last_alt)
    avg_alt = sum(last_alt) / len(last_alt)

    bottom_3_rows = (get_lidar_average(lidar, 5) + get_lidar_average(lidar, 6) + get_lidar_average(lidar, 7)) / 3
    rise_ahead = bottom_3_rows < 30
    dip_ahead = bottom_3_rows > 200

    # bottom row more important
    bottom_2_row_std = (get_lidar_std(lidar, 6) / get_lidar_average(lidar, 6) + 9 * get_lidar_std(lidar,
                                                                                                  7) / get_lidar_average(
        lidar, 7)) / 10

    bumpy_std = bottom_2_row_std > 0.35

    return ((abs(max_alt - min_alt) / avg_alt) < 0.2) and not rise_ahead and not dip_ahead and not bumpy_std


# -------------------------------------
def outside_dir_margin(cur_bearing, target_bearing, margin):
    if cur_bearing + math.pi * 1.01 < target_bearing:
        cur_bearing += 2 * math.pi
    elif cur_bearing - math.pi * 1.01 > target_bearing:
        cur_bearing -= 2 * math.pi

    return cur_bearing - target_bearing > margin or cur_bearing - target_bearing < -margin


# -------------------------------------
def turn_wheels_for_circle(steer_amt):
    if Flipped:
        torque_amt = -steer_amt

    robot.set_tire_steering("FrontLeft", steer_amt * 57.3)  # convert to angles
    robot.set_tire_steering("FrontRight", -steer_amt * 57.3)  # convert to angles
    robot.set_tire_steering("BackLeft", -steer_amt * 57.3)  # convert to angles
    robot.set_tire_steering("BackRight", steer_amt * 57.3)  # convert to angles


# -------------------------------------
def run_wheels_for_pirouette(torque_amt):
    robot.set_tire_torque("BackLeft", torque_amt)
    robot.set_tire_torque("BackRight", -torque_amt)
    robot.set_tire_torque("FrontLeft", torque_amt)
    robot.set_tire_torque("FrontRight", -torque_amt)


# -------------------------------------
def pirouette(cur_bearing, target_bearing):
    wheel_angle = math.pi / 4

    if abs(cur_bearing - target_bearing) > math.pi * 1.01:  # i.e. if it is shorter to go the other way
        if target_bearing < 0:
            target_bearing += 2 * math.pi  # Not rad safe right now but it works
        else:
            target_bearing -= 2 * math.pi

    if cur_bearing > target_bearing:
        turn_wheels_for_circle(wheel_angle)
        run_wheels_for_pirouette(-max_torque / 3)
    else:
        turn_wheels_for_circle(wheel_angle)
        run_wheels_for_pirouette(max_torque / 3)


# -------------------------------------
def hold_bearing_and_speed(target_bearing, margin):
    global last_gps
    global then
    global goal_direction
    global current_bearing
    global Flipped
    global Pirouetting
    global any_fails
    global current_speed
    global last_speed

    if robot.info.gyroscope.is_upside_down and (not Flipped):
        print("Flipping!")
        Flipped = True
        any_fails = True
        robot.flip_coordinates()
    elif (not robot.info.gyroscope.is_upside_down) and Flipped:
        print("Flipping!")
        Flipped = False
        any_fails = True
        robot.flip_coordinates()

    robot_vector = get_bearing_vec(robot.info.gyroscope.forward)

    current_bearing = get_hillless_bearing(robot_vector)

    robot_gps = robot.info.gps.position
    current_gps = np.array([robot_gps.x, robot_gps.y, robot_gps.z])

    now = robot.info.timestamp / 1000.0
    if now > then:

        current_speed = vec_dist(last_gps, current_gps) / (now - then)

    else:

        current_speed = last_speed

    if current_speed < 1.5 and (outside_dir_margin(current_bearing, target_bearing, 2 * math.pi / 3) or (
            outside_dir_margin(current_bearing, target_bearing, math.pi / 3) and Pirouetting)):
        pirouette(current_bearing, target_bearing)
        Pirouetting = True
        might_print(1, "Cur_B = " + str(current_bearing) + "  Tar_B =" + str(target_bearing) + "   Mar = " + str(
            2 * margin) + " Speed= ", current_speed)

    else:

        Pirouetting = False
        keep_speed(current_speed, current_bearing, target_bearing, margin)
        steer_control(current_speed, current_bearing, target_bearing, margin)

    then = now
    last_speed = current_speed
    last_gps = current_gps


# ---------------------------------------
def Reverse_away_if_necessary():
    print("REVERSING!")

    enemy = robot.info.radar.it_ping

    IT_bearing = get_bearing(get_bearing_vec(enemy))

    en_dist = get_dist_to_foe(enemy)

    behind = behind_me(IT_bearing)

    if not behind and en_dist < 5:
        steer(.5)  # ideally you'd be smarter about which way to turn here
        set_torque(-max_torque * 1.1)
        time.sleep(1)


# -----------------------------------------
def which_opponent_closest(enemies_arr):
    return 0


# -----------------------------------------
def total_lidar_space(lidar, start_row, past_last_row, start_col, past_last_col):
    temp = np.array(lidar[start_row:past_last_row, start_col:past_last_col])

    return np.sum(temp)


# -----------------------------------------
def rough_test_lidar_space(lidar, start_row, past_last_row, start_col, past_last_col):
    temp = np.array(lidar[start_row:past_last_row, start_col:past_last_col])
    max = np.amax(temp)
    temp = np.array(temp / max)
    temp_std = np.std(temp)

    return temp_std


# -----------------------------------------
def bias_off_roll_and_clear_space(goal_dir):
    global current_speed

    roll_adj = (-1 * roll()) / 20
    roll_row_adj = max(min(int(round(roll() * 3)), 3), -3)
    lidar = np.array(get_front_high_lidar())

    center_total_space = total_lidar_space(lidar, 3, 7, 4, 8)
    left_total_space = total_lidar_space(lidar, 3 - roll_row_adj, 7 - roll_row_adj, 0, 4)
    right_total_space = total_lidar_space(lidar, 3 + roll_row_adj, 7 + roll_row_adj, 8, 12)
    if left_total_space > center_total_space or right_total_space > center_total_space:
        net_space = (right_total_space - left_total_space) / (right_total_space + left_total_space)
    else:
        net_space = 0

    center_amount_smooth = 1.01 - rough_test_lidar_space(lidar, 3, 7, 0, 4)
    left_amount_smooth = 1.01 - rough_test_lidar_space(lidar, 3 - roll_row_adj, 7 - roll_row_adj, 0, 4)
    right_amount_smooth = 1.01 - rough_test_lidar_space(lidar, 3 + roll_row_adj, 7 + roll_row_adj, 8, 12)
    if left_amount_smooth > center_amount_smooth or right_amount_smooth > center_amount_smooth:
        net_smooth = (right_amount_smooth - left_amount_smooth) / (right_amount_smooth + left_amount_smooth)
    else:
        net_smooth = 0

    lidar_bias = (2 * net_space + net_smooth) / 60
    might_print(1, "Lidar bias on direction = " + str(lidar_bias) + " and adj for roll is ", roll_adj)

    # adj_for_speed = 1
    if current_speed > 30 or current_speed < 0:
        print("WHAT ?!?!?!  : (  speed = ", current_speed)

    adj_for_speed = max(current_speed ** 0.5, 1)
    # why is this making it drive slower?

    adj = rad_safe_add(goal_dir, adj_for_speed * (lidar_bias + roll_adj))

    return adj


# -----------------------------------------
def Play_Tag():
    global goal_direction

    LAST_IT = robot.info.isIt

    while True:

        IT = robot.info.isIt

        if IT and len(robot.info.radar.pings) > 0:
            index = which_opponent_closest(robot.info.radar.pings)
            original_goal_direction = charge_enemy(
                robot.info.radar.pings[index])  # if there is more than one you should go after the closest!

            if in_canyon():
                goal_direction = goal_direction  # was set in in_canyon
            elif in_clear_space() and get_dist_to_foe(robot.info.radar.pings[index]) > 30:
                goal_direction = bias_off_roll_and_clear_space(goal_direction)
                goal_direction = return_angle_compromise(original_goal_direction, goal_direction, 3)
            elif get_dist_to_foe(robot.info.radar.pings[index]) < 30:
                goal_direction = original_goal_direction
            else:
                goal_direction = bias_off_roll_and_clear_space(goal_direction)
                goal_direction = return_angle_compromise(original_goal_direction, goal_direction, 1)

        elif len(robot.info.radar.pings) > 0:
            original_goal_direction = flee_enemy(robot.info.radar.it_ping)

            if in_canyon():
                goal_direction = goal_direction  # was set in in_canyon
            elif in_clear_space() and get_dist_to_foe(robot.info.radar.pings[index]) > 100:
                goal_direction = bias_off_roll_and_clear_space(goal_direction)
                goal_direction = return_angle_compromise(original_goal_direction, goal_direction, 5)
            elif get_dist_to_foe(robot.info.radar.pings[index]) < 100:
                goal_direction = original_goal_direction
            else:
                goal_direction = bias_off_roll_and_clear_space(goal_direction)
                goal_direction = return_angle_compromise(original_goal_direction, goal_direction, 1)

        hold_bearing_and_speed(goal_direction, .2)

        if not IT and LAST_IT:
            Reverse_away_if_necessary()

        LAST_IT = IT

        time.sleep(0.0011)


# ---------------------------------------
def its_been_x_since_a_save(delay):
    global savetime

    current = int(round(time.time()))

    return (current - savetime) > delay  # i.e. its been more than 10 seconds


# ---------------------------------------
def savemaps():
    global elevation_high_map
    global elevation_low_map
    global avg_speed_map
    global num_fails_map
    global num_visits_map
    global clear_space_map
    global current_torque

    set_torque(current_torque / 10)

    np.savetxt("elevation_high_map.txt", elevation_high_map)

    np.savetxt("elevation_low_map.txt", elevation_low_map)

    np.savetxt("avg_speed_map.txt", avg_speed_map)

    np.savetxt("num_fails_map.txt", num_fails_map)

    np.savetxt("num_visits_map.txt", num_visits_map)

    np.savetxt("clear_space_map.txt", clear_space_map)


# ---------------------------------------
def update_map():
    global elevation_high_map
    global elevation_low_map
    global avg_speed_map
    global num_fails_map
    global num_visits_map
    global clear_space_map
    global last_speed
    global any_fails

    x = round((robot.info.gps.position.x) / x_map_reduce) + half_map_width
    y = round((robot.info.gps.position.y) / y_map_reduce) + half_map_height

    cur_el = robot.info.gps.position.z
    cur_speed = last_speed
    if cur_speed > 30:
        cur_speed = 30

    num_visits = num_visits_map[x][y]

    old_avg_speed = avg_speed_map[x][y]
    new_avg_speed = (old_avg_speed * num_visits + cur_speed) / (num_visits + 1)
    avg_speed_map[x][y] = new_avg_speed

    if cur_el > elevation_high_map[x][y] or num_visits == 0:
        elevation_high_map[x][y] = cur_el
    if cur_el < elevation_low_map[x][y] or num_visits == 0:
        elevation_low_map[x][y] = cur_el

    if in_clear_space():
        clear_space_map[x][y] += 1

    if any_fails:
        num_fails_map[x][y] += 1
        any_fails = False

    num_visits_map[x][y] += 1

    might_print(1, "Passability for (" + str(x) + "," + str(y) + ") = ", passability_of_area(x, y))


# ---------------------------------------
def dir_to_get_to_area(x, y):
    pos = robot.info.gps.position
    cur_pos = np.array([pos.x, pos.y, pos.z])
    desired_pos = np.array([(x - half_map_width) * x_map_reduce, (y - half_map_height) * y_map_reduce, 0])

    angle = -1 * (np.arctan2(desired_pos[1] - cur_pos[1], desired_pos[0] - cur_pos[0]) - math.pi / 2)
    if angle > math.pi:
        angle -= 2 * math.pi

    return angle


# ---------------------------------------
def return_angle_compromise(angle1, angle2, weight_towards_angle2):
    if abs(angle1 - angle2) < math.pi:
        comp_angle = (angle1 + weight_towards_angle2 * angle2) / (weight_towards_angle2 + 1)
    elif angle1 > angle2:
        angle2 += 2 * math.pi
        comp_angle = (angle1 + weight_towards_angle2 * angle2) / (weight_towards_angle2 + 1)
        if comp_angle > math.pi:
            comp_angle -= 2 * math.pi
    else:
        angle1 += 2 * math.pi
        comp_angle = (angle1 + weight_towards_angle2 * angle2) / (weight_towards_angle2 + 1)
        if comp_angle > math.pi:
            comp_angle -= 2 * math.pi

    return comp_angle


# ---------------------------------------
def in_clear_space():
    lidar = np.array(robot.info.lidar.distance_matrix)
    total_space = total_lidar_space(lidar, 0, 4, 0, 72)

    flat = 0

    if check_not_bumpy():
        flat += 1

    if abs(slope()) < 0.15:
        flat += 1

    if abs(roll()) < 0.15:
        flat += 1

    if total_space > 160 * 4 * 72:
        flat += 1
        if total_space > 210 * 4 * 72:
            flat += 1

    return flat > 3


# ---------------------------------------
def get_canyon_amount(lidar, start_row, stop_row, iminus2, iminus1, i, iplus1, iplus2):
    if iminus1 < 0:
        iminus1 = 72 + iminus1

    if iminus2 < 0:
        iminus2 = 72 + iminus2

    if iplus1 > 71:
        iplus1 = iplus1 - 72

    if iplus2 > 71:
        iplus2 = iplus2 - 72

    sum = 0

    for row in range(start_row, stop_row):
        sum += lidar[row][iminus2]
        sum += lidar[row][iminus1]
        sum += lidar[row][i]
        sum += lidar[row][iplus1]
        sum += lidar[row][iplus2]

    return sum


# ---------------------------------------
def which_angle_closer(cur_angle, angle1, angle2):
    if abs(cur_angle - angle1) > math.pi:
        angle1_dist = 2 * math.pi - abs(cur_angle - angle1)
    else:
        angle1_dist = abs(cur_angle - angle1)

    if abs(cur_angle - angle2) > math.pi:
        angle2_dist = 2 * math.pi - abs(cur_angle - angle2)
    else:
        angle2_dist = abs(cur_angle - angle2)

    if angle1_dist < angle2_dist:
        return angle1
    else:
        return angle2


# ---------------------------------------
def in_canyon():
    global is_in_canyon
    global goal_direction
    global current_bearing
    global original_goal_direction
    global last_canyon_direction
    global current_bearing

    lidar = get_canyon_lidar()
    can_arr = np.zeros(72)
    can_arr_reflect = np.zeros(36)

    for i in range(0, 72):
        can_arr[i] = get_canyon_amount(lidar, 0, 7, i - 2, i - 1, i, i + 1, i + 2)

    for i in range(0, 36):
        can_arr_reflect[i] = (can_arr[i] + can_arr[i + 36]) / 2

    max_el = np.amax(can_arr_reflect)
    min_el = np.amin(can_arr_reflect)

    result = np.where(can_arr_reflect == np.amax(can_arr_reflect))
    dir_index_max = result[0][0]

    result = np.where(can_arr_reflect == np.amin(can_arr_reflect))
    dir_index_min = result[0][0]

    can_angle = abs(dir_index_min - dir_index_max)

    might_print(1, " _______ Ratio = " + str(max_el / min_el) + " and can_angle = ", can_angle)

    if (max_el / min_el > 2.2 or (max_el / min_el > 1.5 and is_in_canyon)) and (can_angle > 13 and can_angle < 23):
        canyon_rel_dir = (dir_index_max * 2 * math.pi) / 72
        canyon_absl_dir = rad_safe_add(current_bearing, canyon_rel_dir)
        canyon_absl_opp_dir = rad_safe_add(current_bearing, canyon_rel_dir + math.pi)
        # print("Canyon Test Max is ",max_el," and Max Index = ",dir_index_max," and Min is ",min_el," and Min Index is ",dir_index_min)
        # print("In Canyon!! and can dir = ",canyon_absl_dir," and can dir opp = ",canyon_absl_opp_dir," and Current Dir = ",current_bearing," and Goal Dir = ",goal_direction,"and Org Dir = ",original_goal_direction)
        if is_in_canyon:
            goal_direction = which_angle_closer(goal_direction, canyon_absl_dir, canyon_absl_opp_dir)
            goal_direction = return_angle_compromise(goal_direction, last_canyon_direction, 10)
        else:
            goal_direction = which_angle_closer(original_goal_direction, canyon_absl_dir, canyon_absl_opp_dir)

        # print("###  goal direction just got canyonized to ",goal_direction)
        last_canyon_direction = goal_direction
        is_in_canyon = True
        return True
    else:
        is_in_canyon = False

        return False


# ---------------------------------------
def Fight():
    global goal_direction

    while True:

        if len(robot.info.radar.pings) > 0:
            index = which_opponent_closest(robot.info.radar.pings)
            original_goal_direction = charge_enemy(
                robot.info.radar.pings[index])  # if there is more than one you should go after the closest!

            if in_canyon():
                goal_direction = goal_direction  # was set in in_canyon
            elif in_clear_space() and get_dist_to_foe(robot.info.radar.pings[index]) > 50:
                goal_direction = bias_off_roll_and_clear_space(goal_direction)
                goal_direction = return_angle_compromise(original_goal_direction, goal_direction, 5)
            elif get_dist_to_foe(robot.info.radar.pings[index]) < 50:
                goal_direction = original_goal_direction
            else:
                goal_direction = bias_off_roll_and_clear_space(goal_direction)
                goal_direction = return_angle_compromise(original_goal_direction, goal_direction, 1)

        hold_bearing_and_speed(goal_direction, .2)

        time.sleep(0.0011)


# ---------------------------------------
def Explore():
    global savetime
    global goal_direction
    global original_goal_direction

    x, y = pick_unvisited_area()
    print("X and Y are ", x, y)
    goal_direction = dir_to_get_to_area(x, y)
    original_goal_direction = goal_direction
    print("Goal Direction (also Original Dir) = ", goal_direction)

    while True:

        robot.lock_info()

        original_goal_direction = dir_to_get_to_area(x, y)

        if in_canyon():
            goal_direction = goal_direction  # was set in in_canyon
        elif in_clear_space():
            goal_direction = bias_off_roll_and_clear_space(goal_direction)
            might_print(1, "In Clear Space:  Goal Dir = " + str(goal_direction) + "   Orig Dir = ",
                        original_goal_direction)
            goal_direction = return_angle_compromise(original_goal_direction, goal_direction, 6)
        else:
            goal_direction = bias_off_roll_and_clear_space(goal_direction)
            might_print(1, "Just Dealing:  Goal Dir = " + str(goal_direction) + "   Orig Dir = ",
                        original_goal_direction)
            goal_direction = return_angle_compromise(original_goal_direction, goal_direction, 300)

        hold_bearing_and_speed(goal_direction, 0.2)

        update_map()

        robot.unlock_info()

        if (last_speed < 3) and (its_been_x_since_a_save(30)) and in_clear_space():
            print("SAVING!  ********")
            savemaps()
            savetime = int(round(time.time()))
        else:
            time.sleep(0.0011)


# ----------- MAIN LOOP! ----------------

setup_maps(start_maps_over)

if robot.info.gamemode == "Freeplay":
    base_speed = 1.8
    Fight()

elif robot.info.gamemode == "Singleplayer":
    base_speed = 1.4
    Explore()

else:  # "Classic Tag"
    base_speed = 2.2
    Play_Tag()

robot.disconnect()
