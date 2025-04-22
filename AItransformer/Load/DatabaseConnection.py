from abc import ABC, abstractmethod


class IDatabaseConnection(ABC):

    @abstractmethod
    def connect(self):
        pass

    @abstractmethod
    def write(self, json_data):
        pass

    def close(self):
        pass
