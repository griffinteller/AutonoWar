import win32pipe, win32file, threading

PIPE_NAME = "RobotInfoPipe"


def send_event_thread():

    event_buffer = [str(i) + "\n" for i in range(300)]
    event_buffer[-1] = event_buffer[-1][:-1]

    pipe = win32pipe.CreateNamedPipe(

        "\\\\.\\pipe\\" + PIPE_NAME,
        win32pipe.PIPE_ACCESS_DUPLEX,
        win32pipe.PIPE_TYPE_MESSAGE | win32pipe.PIPE_READMODE_MESSAGE | win32pipe.PIPE_WAIT,
        1, 65536, 65536,
        0,
        None

    )

    try:

        print("Waiting for client...\n")
        win32pipe.ConnectNamedPipe(pipe, None)
        print("Client connected.\n")

        while True:

            text = win32file.ReadFile(pipe, 64*1024)
            print(text)

    finally:

        win32file.CloseHandle(pipe)


thread = threading.Thread(target=send_event_thread())
thread.start()