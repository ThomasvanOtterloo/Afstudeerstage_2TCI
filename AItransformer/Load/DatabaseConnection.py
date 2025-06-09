from abc import ABC, abstractmethod


class IDatabaseConnection(ABC):

    @abstractmethod
    def connect(self):
        pass

    @abstractmethod
    def write_ad(self, json_data):
        pass

