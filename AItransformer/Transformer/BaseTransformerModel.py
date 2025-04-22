import torch
import time
from unsloth import FastLanguageModel
from unsloth.chat_templates import get_chat_template

from ITransformer import ITransformer

class BaseTransformerModel(ITransformer):
    def __init__(self):
        with open("Transformer/system_prompt.txt", "r") as f:
            self.system_prompt = f.read()

        self.model, self.tokenizer = FastLanguageModel.from_pretrained(
            model_name="unsloth/llama-3-8b-Instruct",
            max_seq_length=2048,
            dtype=None,
        )
        FastLanguageModel.for_inference(self.model)
        self.tokenizer = get_chat_template(
            self.tokenizer,
            chat_template="chatml",
            mapping={"role": "from", "content": "value", "user": "human", "assistant": "gpt"},
            map_eos_token=True,
        )

    def transformData(self, message):
        messages = [
            {"from": "system", "value": self.system_prompt},
            {"from": "human", "value": message},
        ]

        inputs = self.tokenizer.apply_chat_template(
            messages,
            tokenize=True,
            add_generation_prompt=True,
            return_tensors="pt",
        ).to(self.model.device)

        outputs = self.model.generate(
            input_ids=inputs,
            max_new_tokens=200,
            do_sample=True,
            top_p=0.95,
            temperature=0.05,
        )

        output_text = self.tokenizer.decode(outputs[0], skip_special_tokens=True)
        if "<|im_start|>assistant" in output_text:
            response_normal = output_text.split("<|im_start|>assistant")[-1].strip()
        else:
            response_normal = output_text.strip()

        return response_normal
