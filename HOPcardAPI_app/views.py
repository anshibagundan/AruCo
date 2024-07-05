from rest_framework import viewsets
from rest_framework.decorators import action
from rest_framework.response import Response
from .models import Quiz, Action, Quiz_select, Act_select, Quiz_TF, Act_TF, Player, LR
from .serializers import QuizSerializer, ActionSerializer, QuizSelectSerializer, ActSelectSerializer, QuizTFSerializer, ActTFSerializer, PlayerSerializer,LRSerializer

class QuizViewSet(viewsets.ReadOnlyModelViewSet):
    queryset = Quiz.objects.all()
    serializer_class = QuizSerializer

    def get_queryset(self):
        queryset = super().get_queryset()
        difficulty = self.request.query_params.get('difficulty', None)
        if difficulty is not None:
            queryset = queryset.filter(difficulty=difficulty)
        return queryset

class ActionViewSet(viewsets.ReadOnlyModelViewSet):
    queryset = Action.objects.all()
    serializer_class = ActionSerializer

    def get_queryset(self):
        queryset = super().get_queryset()
        difficulty = self.request.query_params.get('difficulty', None)
        if difficulty is not None:
            queryset = queryset.filter(difficulty=difficulty)
        return queryset



class QuizSelectViewSet(viewsets.ModelViewSet):
    queryset = Quiz_select.objects.all()
    serializer_class = QuizSelectSerializer

    @action(detail=False, methods=['delete'])
    def destroy_all(self, request):
        Quiz_select.objects.all().delete()
        return Response(status=204)

class ActSelectViewSet(viewsets.ModelViewSet):
    queryset = Act_select.objects.all()
    serializer_class = ActSelectSerializer

    @action(detail=False, methods=['delete'])
    def destroy_all(self, request):
        Act_select.objects.all().delete()
        return Response(status=204)

class QuizTFViewSet(viewsets.ModelViewSet):
    queryset = Quiz_TF.objects.all()
    serializer_class = QuizTFSerializer

    @action(detail=False, methods=['delete'])
    def destroy_all(self, request):
        Quiz_TF.objects.all().delete()
        return Response(status=204)

class ActTFViewSet(viewsets.ModelViewSet):
    queryset = Act_TF.objects.all()
    serializer_class = ActTFSerializer

    @action(detail=False, methods=['delete'])
    def destroy_all(self, request):
        Act_TF.objects.all().delete()
        return Response(status=204)

class PlayerViewSet(viewsets.ModelViewSet):
    queryset = Player.objects.all()
    serializer_class = PlayerSerializer

    @action(detail=False, methods=['delete'])
    def destroy_all(self, request):
        Player.objects.all().delete()
        return Response(status=204)

class LRViewSet(viewsets.ModelViewSet):
    queryset = LR.objects.all()
    serializer_class = LRSerializer

    @action(detail=False, methods=['delete'])
    def destroy_all(self, request):
        LR.objects.all().delete()
        return Response(status=204)