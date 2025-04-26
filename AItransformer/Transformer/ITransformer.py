# ITransformer.py
from abc import ABC, abstractmethod


class ITransformer(ABC):
    @abstractmethod
    def transformData(self, message: str) -> str:
        pass
