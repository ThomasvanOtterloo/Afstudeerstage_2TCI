import json
from pathlib import Path
import torch
from transformers import (
    DistilBertForSequenceClassification,
    DistilBertTokenizerFast
)
from torch.nn.functional import softmax
from Transformer.TransformerDecorator import TransformerDecorator


class BrandIdentifierDecorator(TransformerDecorator):
    CONFIDENCE_THRESHOLD = 0.8

    def __init__(self, wrapped, model_path="../Model/ref2brand"):
        super().__init__(wrapped)

        # folder = Path(model_path).expanduser().resolve()
        folder = Path(__file__).parent.joinpath(model_path).resolve()
        # — load model & tokenizer exactly as before —
        self.model = DistilBertForSequenceClassification.from_pretrained(
            str(folder), local_files_only=True
        )
        self.tokenizer = DistilBertTokenizerFast.from_pretrained(
            str(folder), local_files_only=True
        )

        # —— now override the label mapping ——
        # (1) load your original brand list
        brands_path = folder.parent / "brands.json"
        with open(brands_path, encoding="utf-8") as f:
            raw = json.load(f)
        # (2) get a sorted, unique list of brand names
        unique_brands = sorted({entry["brand"] for entry in raw})
        # (3) build an int→string map
        self.id2label = {i: brand for i, brand in enumerate(unique_brands)}
        # (4) and—if you ever care—string→int as well
        self.label2id = {v: k for k, v in self.id2label.items()}

        # finally stick it back into your model.config so that
        # if anything downstream introspects config.id2label they
        # also get the right mapping.
        self.model.config.id2label = {str(i): b for i, b in self.id2label.items()}
        self.model.config.label2id = self.label2id

        self.model.eval()

    def transformData(self, message: str | dict | list) -> str:
        # 1) Run the wrapped transformer
        base_out = self._wrapped.transformData(message)

        # 2) Deserialize if it’s a JSON string
        if isinstance(base_out, str):
            try:
                payload = json.loads(base_out)
            except json.JSONDecodeError:
                return base_out
        else:
            payload = base_out

        # 3) Normalize to a list
        records = payload if isinstance(payload, list) else [payload]

        # 4) Process every record
        for record in records:
            ref = record.get("ReferenceNumber")
            if not ref:
                continue

            enc = self.tokenizer(
                [ref], padding=True, truncation=True, max_length=32, return_tensors="pt"
            )
            with torch.no_grad():
                logits = self.model(**enc).logits
                probs = softmax(logits, dim=1)[0]

            idx = int(probs.argmax())
            conf = float(probs[idx])
            candidate = self.id2label[idx]

            # overwrite only if it’s confident
            if conf >= self.CONFIDENCE_THRESHOLD:
                record["Brand"] = candidate

            # <-- this print will now fire for *each* record
            print(f"BrandIdentifierDecorator: {ref} → {candidate} ({conf:.2f})")

        # 5) only after the loop do we serialize & return
        return json.dumps(records)