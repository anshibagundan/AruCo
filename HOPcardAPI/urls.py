"""
URL configuration for HOPcardAPI project.

The `urlpatterns` list routes URLs to views. For more information please see:
    https://docs.djangoproject.com/en/4.2/topics/http/urls/
Examples:
Function views
    1. Add an import:  from my_app import views
    2. Add a URL to urlpatterns:  path('', views.home, name='home')
Class-based views
    1. Add an import:  from other_app.views import Home
    2. Add a URL to urlpatterns:  path('', Home.as_view(), name='home')
Including another URLconf
    1. Import the include() function: from django.urls import include, path
    2. Add a URL to urlpatterns:  path('blog/', include('blog.urls'))
"""
from django.urls import include, path
from rest_framework import routers
from HOPcardAPI_app.views import QuizViewSet, ActionViewSet, QuizSelectViewSet, ActSelectViewSet, QuizTFViewSet, ActTFViewSet

router = routers.DefaultRouter()
router.register(r'quizzes', QuizViewSet)
router.register(r'actions', ActionViewSet)
router.register(r'quiz-selects', QuizSelectViewSet)
router.register(r'act-selects', ActSelectViewSet)
router.register(r'quiz-tfs', QuizTFViewSet)
router.register(r'act-tfs', ActTFViewSet)

urlpatterns = [
    path('', include(router.urls)),
]