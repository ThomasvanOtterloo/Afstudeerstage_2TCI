# TransformerDecorator.py
from Transformer.ITransformer import ITransformer


class TransformerDecorator(ITransformer):
    def __init__(self, transformer: ITransformer):
        self._wrapped = transformer

    def transformData(self, message: str) -> str:
        return self._wrapped.transformData(message)
