package usecase

import (
	"HOPcardAPI/domain/models"
	"HOPcardAPI/domain/repositories"
)

type QuizUseCase interface {
	GetQuizByID(code int) (*models.Quiz, error)
}

type quizUseCase struct {
	quizRepository repositories.QuizRepository
}

func NewQuizUseCase(quizrepo repositories.QuizRepository) QuizUseCase {
	return &quizUseCase{
		quizRepository: quizrepo,
	}
}

func (uc *quizUseCase) GetQuizByID(id int) (*models.Quiz, error) {
	quiz, err := uc.quizRepository.FindByID(id)
	if err != nil {
		return nil, err
	}
	return quiz, nil
}
