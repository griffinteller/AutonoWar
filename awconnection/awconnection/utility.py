import os
import platform

if platform.system() == "Windows":

    import win32file


MESSAGE_SEPARATOR_BYTES = b";"
MESSAGE_SEPARATOR_STR = ";"


class PosixFifo(object):

    """Wrapper for a posix fifo.

    Attributes
    ----------
    stream : io.TextIO
        The underlying stream
    """

    def __init__(self, name, mode):

        self.__path = "/tmp/" + name

        self.__mode = mode

        self.__init_fifo()

        self.stream = open(self.__path, mode)

    def temp_close(self):

        self.stream.close()

    def re_init(self):

        self.stream = open(self.__path, self.__mode)

    def __init_fifo(self):

        try:

            os.mkfifo(self.__path)

        except OSError:

            pass


def get_most_recent_message_windows(pipe_handle):

    """Retreives the most recent valid message from a named pipe. If there are none, it returns an empty string

    Parameters
    ----------
    pipe_handle : PyHandle
        The handle to the pipe

    Returns
    -------
    str
    """

    pipe_message_text = win32file.ReadFile(pipe_handle, 64 * 1024)[1]
    pipe_messages = pipe_message_text.split(MESSAGE_SEPARATOR_BYTES)

    if len(pipe_messages) == 0:

        return b""

    elif len(pipe_messages[-1]) == 0 or pipe_messages[-1][-1] != MESSAGE_SEPARATOR_BYTES:

        return pipe_messages[-2]

    else:

        return pipe_messages[-1]


def get_most_recent_message_posix(fifo):

    """Retrieves the most recent valid message from a named pipe. If there are none, it returns an empty string.

    Parameters
    ----------
    fifo : PosixFifo
        The handle to the pipe

    Returns
    -------
    str
    """

    pipe_message_text = fifo.stream.read()
    pipe_messages = pipe_message_text.split(MESSAGE_SEPARATOR_STR)

    if len(pipe_messages) == 0:

        return ""  # reached end of file

    elif len(pipe_messages) == 1:

        return ""  # something funky happened

    else:

        return pipe_messages[-2] # every message ends with a semicolon, so last string is just and empty string
