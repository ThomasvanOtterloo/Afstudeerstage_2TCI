# Transformer/TransformerDecorator.py
from Transformer.ITransformer import ITransformer


class TransformerDecorator(ITransformer):
    def __init__(self, wrapped: ITransformer):
        print("TransformerDecorator.__init__")
        self._wrapped = wrapped

    def transformData(self, message: str) -> str:
        # just forward by default
        return self._wrapped.transformData(message)
