# routing.py
from django.urls import re_path
from . import consumers,consumers_start, consumers_quiz

websocket_urlpatterns = [
    re_path(r'ws/hop/$', consumers.HOPConsumer.as_asgi()),
    re_path(r'ws/hop/start/$', consumers_start.HOPConsumer_start.as_asgi()),
    re_path(r'ws/hop/quiz/$', consumers_quiz.HOPConsumer_quiz.as_asgi()),
]
