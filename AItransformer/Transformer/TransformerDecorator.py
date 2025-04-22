class TransformerDecorator:
    """
    A decorator for the Transformer class that adds functionality to the transform method.
    """

    def __init__(self, transformer):
        self.transformer = transformer

    def transform(self, data):
        """
        Transforms the data using the transformer instance.
        """
        # Add pre-processing or validation logic here if needed
        transformed_data = self.transformer.transform(data)
        # Add post-processing or logging logic here if needed
        return transformed_data