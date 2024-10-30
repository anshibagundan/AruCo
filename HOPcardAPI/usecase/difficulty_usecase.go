package usecase

import (
	"HOPcardAPI/domain/models"
	"HOPcardAPI/domain/repositories"
)

type GameUsecase struct {
	quizRepo   repositories.QuizRepository
	actionRepo repositories.ActionRepository
}

func NewGameUsecase(qr repositories.QuizRepository, ar repositories.ActionRepository) *GameUsecase {
	return &GameUsecase{
		quizRepo:   qr,
		actionRepo: ar,
	}
}

func (u *GameUsecase) ProcessGameData(difficulty int) (*models.UnityDifficultyMessage, error) {
	quizzes, err := u.quizRepo.FindByDifficulty(difficulty, 3)
	if err != nil {
		return nil, err
	}

	action, err := u.actionRepo.FindOneByDifficulty(difficulty)
	if err != nil {
		return nil, err
	}

	quizIDs := make([]int, len(quizzes))
	for i, quiz := range quizzes {
		quizIDs[i] = quiz.ID
	}

	return &models.UnityDifficultyMessage{
		QuizIDs:  quizIDs,
		ActionID: action.ID,
	}, nil
}
