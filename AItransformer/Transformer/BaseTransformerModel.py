from abc import ABC

import torch
from unsloth import FastLanguageModel
from unsloth.chat_templates import get_chat_template
from Transformer.ITransformer import ITransformer


class BaseTransformerModel(ITransformer):

    def __init__(self):
        # 1.  Load model & tokenizer
        self.model, tok = FastLanguageModel.from_pretrained(
            "Qwen/Qwen2.5-7B-Instruct",
            max_seq_length=4048,
            dtype=None,
        )
        # 2.  Attach the chat-ML template you like
        self.tokenizer = get_chat_template(
            tok,
            chat_template="chatml",
            mapping={
                "role": "from",
                "content": "value",
                "user": "human",
                "assistant": "gpt",
            },
            map_eos_token=True,
        )
        FastLanguageModel.for_inference(self.model)

        # 3.  Cache system prompt as tokens (on the correct device)
        with open("Transformer/system_prompt.txt", encoding="utf-8") as f:
            self.system_prompt = f.read()

        self.system_ids = self.tokenizer.apply_chat_template(
            [{"role": "system", "content": self.system_prompt}],
            tokenize=True,
            add_generation_prompt=False,
            return_tensors="pt",
        ).to(self.model.device)  # <-- stays on GPU/CPU

    # ------------------------------------------------------------------ #
    # Helpers
    # ------------------------------------------------------------------ #
    def _build_inputs(self, user_message: str) -> torch.Tensor:
        """
        Tokenise the new user turn and append to the cached system ids.
        """
        user_ids = self.tokenizer.apply_chat_template(
            [{"role": "user", "content": user_message}],
            tokenize=True,
            add_generation_prompt=True,  # tells model: now you speak
            return_tensors="pt",
        ).to(self.model.device)
        return torch.cat([self.system_ids, user_ids], dim=1)

    # ------------------------------------------------------------------ #
    # Public API
    # ------------------------------------------------------------------ #
    def transformData(
            self,
            message: str,
            max_new_tokens: int = 4048,
            top_p: float = 0.95,
            temperature: float = 0.15,
    ) -> str:
        input_ids = self._build_inputs(message)

        with torch.no_grad():
            output = self.model.generate(
                input_ids=input_ids,
                max_new_tokens=max_new_tokens,
                do_sample=True,
                top_p=top_p,
                temperature=temperature,
            )

        # strip the prompt part so only the assistant answer remains
        generated_ids = output[0][input_ids.shape[1]:]  # drop prefix tokens
        text = self.tokenizer.decode(
            generated_ids, skip_special_tokens=True
        ).strip()

        return text
