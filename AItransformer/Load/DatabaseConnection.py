from abc import ABC, abstractmethod


class IDatabaseConnection(ABC):

    @abstractmethod
    def connect(self):
        pass

    @abstractmethod
    def dummy_write(self):
        pass

    def close(self):
        pass
