package usecase

import (
	"HOPcardAPI/domain/models"
	"HOPcardAPI/domain/repositories"
)

type DifficultyUsecase struct {
	quizRepo   repositories.QuizRepository
	actionRepo repositories.ActionRepository
}

func NewDifficultyUsecase(qr repositories.QuizRepository, ar repositories.ActionRepository) *DifficultyUsecase {
	return &DifficultyUsecase{
		quizRepo:   qr,
		actionRepo: ar,
	}
}

func (u *DifficultyUsecase) ProcessDifficultyData(difficulty int) (*models.UnityDifficultyMessage, error) {
	// difficultyに応じたクイズを3つ取得
	quizzes, err := u.quizRepo.FindByDifficulty(difficulty, 3)
	if err != nil {
		return nil, err
	}

	// difficultyに応じたアクションを取得
	action, err := u.actionRepo.FindOneByDifficulty(difficulty)
	if err != nil {
		return nil, err
	}

	// 3つの要素を持つquizIDsを作成
	quizIDs := make([]int, len(quizzes))
	for i, quiz := range quizzes {
		quizIDs[i] = quiz.ID
	}

	// Unityに向けたレスポンスであるUnityDifficultyMessageを作成
	return &models.UnityDifficultyMessage{
		QuizIDs:  quizIDs,
		ActionID: action.ID,
	}, nil
}
