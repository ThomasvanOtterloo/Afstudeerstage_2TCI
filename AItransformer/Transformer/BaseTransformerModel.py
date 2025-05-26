from abc import ABC
import torch
from unsloth import FastLanguageModel
from Transformer.ITransformer import ITransformer


class BaseTransformerModel(ITransformer):
    def __init__(self):
        # 1. Load model & tokenizer via Unsloth
        self.model, self.tokenizer = FastLanguageModel.from_pretrained(
            "unsloth/Qwen3-4B-unsloth-bnb-4bit",
            max_seq_length=4092,
            dtype=None,
        )
        # Prepare model for inference
        FastLanguageModel.for_inference(self.model)

        # 2. Load system prompt
        with open("Transformer/system_prompt.txt", encoding="utf-8") as f:
            self.system_prompt = f.read().strip()

    def transformData(
        self,
        message: str,
        max_new_tokens: int = 4048,
        top_p: float = 0.95,
        temperature: float = 0.65,
    ) -> str:
        # 3. Build the chat template messages (system + user)
        messages = [
            {"role": "system", "content": self.system_prompt},
            {"role": "user", "content": message},
        ]
        # 4. Apply Unsloth chat template to get pure text, disable internal thinking
        prompt_text = self.tokenizer.apply_chat_template(
            messages,
            tokenize=False,
            add_generation_prompt=True,
            enable_thinking=False,
        )
        # 5. Use underlying HF tokenizer to encode
        model_inputs = self.tokenizer([prompt_text], return_tensors="pt").to(self.model.device)

        # 6. Generate
        with torch.no_grad():
            output = self.model.generate(
                **model_inputs,
                max_new_tokens=max_new_tokens,
                do_sample=True,
                top_p=top_p,
                temperature=temperature,
            )

        # 7. Strip prompt tokens so only the assistant reply remains
        generated_ids = output[0][model_inputs.input_ids.shape[1]:]
        content = self.tokenizer.decode(generated_ids, skip_special_tokens=True).strip()
        print("BaseTransformerModel: \n", content)
        return content
