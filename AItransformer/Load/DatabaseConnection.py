from abc import ABC, abstractmethod


class IDatabaseConnection(ABC):

    @abstractmethod
    def connect(self):
        pass

    @abstractmethod
    def write_ad(self, json_data):
        pass

    @abstractmethod
    def write_trader(self, json_data):
        pass

    @abstractmethod
    def write_group(self, json_data):
        pass

    @abstractmethod
    def read(self, query):
        pass

    def close(self):
        pass
