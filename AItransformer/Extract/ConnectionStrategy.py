from abc import ABC, abstractmethod


class ConnectionStrategy(ABC):
    @abstractmethod
    def webhook(self):
        """
        Handle incoming webhook requests.
        """
        pass


