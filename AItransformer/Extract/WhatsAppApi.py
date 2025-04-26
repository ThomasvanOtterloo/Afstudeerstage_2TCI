import threading

import requests
from flask import Flask, request, jsonify


class WhatsAppApi:
    def __init__(self):
        self.app = Flask(__name__)
        self.app.add_url_rule("/webhook", "webhook_listener", self.webhook, methods=["POST"])
        self._listeners = []

    def webhook(self):
        data = request.get_json()
        print("📩 Webhook received:", data)
        for listener in self._listeners:
            listener(data)
        return {"status": "ok"}, 200

    def on_message(self, callback_fn):
        self._listeners.append(callback_fn)

    def start(self, port=5006):
        # Run Flask in a background thread so it doesn't block
        threading.Thread(target=lambda: self.app.run(host="0.0.0.0", port=5006), daemon=True).start()
