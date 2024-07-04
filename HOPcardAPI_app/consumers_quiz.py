import json
from channels.generic.websocket import AsyncWebsocketConsumer

class HOPConsumer_quiz(AsyncWebsocketConsumer):
    async def connect(self):
        await self.channel_layer.group_add("Quiz_group", self.channel_name)
        await self.accept()
        print(f"[DEBUG] WebSocket connection accepted: {self.channel_name}")

    async def disconnect(self, close_code):
        await self.channel_layer.group_discard("Quiz_group", self.channel_name)
        print(f"[DEBUG] WebSocket connection closed: {self.channel_name} with code {close_code}")

    async def receive(self, text_data):
        print(f"[DEBUG] Received raw data: {text_data}")

        if not text_data.strip():
            print("[DEBUG] Received empty message")
            return

        try:
            data = json.loads(text_data)
            print(f"[DEBUG] Received JSON data: {data}")
        except json.JSONDecodeError as e:
            print(f"[DEBUG] JSONDecodeError: {e}")
            return

        # Prepare the response data
        response_data = {
            "quiz": data.get("quiz", "")
        }

        # Send the response data back to the client
        await self.send(text_data=json.dumps(response_data))
        print(f"[DEBUG] Sent coordinates back to client: {response_data}")

        # Broadcast the received data to the group
        await self.channel_layer.group_send(
            "HOP_group",
            {
                'type': 'hop_message',
                'message': response_data
            }
        )

    async def hop_message(self, event):
        message = event['message']
        await self.send(text_data=json.dumps(message))
        print(f"[DEBUG] Broadcasted message to group: {message}")
