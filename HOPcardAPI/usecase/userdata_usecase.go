package usecase

import (
	"HOPcardAPI/domain/models"
	"HOPcardAPI/domain/repositories"
)

type UserDataUsecase interface {
	SaveUserData(uuid string, ratio float64, distance float64) (string, error)
	GetUserDataByUuid(uuid string) (*UserDataResponse, error)
	SaveUserQuizResult(uuid string, quizID int, correct bool) error
	SaveUserActionResult(uuid string, actionID int, correct bool) error
}

type userDataUsecase struct {
	quizRepo       repositories.QuizRepository
	userDataRepo   repositories.UserDataRepository
	userQuizRepo   repositories.UserQuizResultRepository
	userActionRepo repositories.UserActionResultRepository
}

// NewUserDataUsecase は UserDataUsecase の新しいインスタンスを返します
func NewUserDataUsecase(quizRepo repositories.QuizRepository, userDataRepo repositories.UserDataRepository, userQuizRepo repositories.UserQuizResultRepository, userActionRepo repositories.UserActionResultRepository) UserDataUsecase {
	return &userDataUsecase{
		quizRepo:       quizRepo,
		userDataRepo:   userDataRepo,
		userQuizRepo:   userQuizRepo,
		userActionRepo: userActionRepo,
	}
}

// SaveUserData はユーザーデータを保存または更新します
func (uc *userDataUsecase) SaveUserData(uuid string, ratio float64, distance float64) (string, error) {
	userData, err := uc.userDataRepo.FindByUuid(uuid)
	if err != nil {
		newID, err := uc.userDataRepo.Save(uuid, ratio, distance)
		if err != nil {
			return "", err
		}
		return newID, nil
	}

	newRatio := (userData.Ratio*float64(userData.ChangeCount+1) + ratio) / float64(userData.ChangeCount+2)
	newDistance := userData.Distance + distance
	newChangeCount := userData.ChangeCount + 1

	userData.Ratio = newRatio
	userData.Distance = newDistance
	userData.ChangeCount = newChangeCount

	updatedID, err := uc.userDataRepo.Update(userData)
	if err != nil {
		return "", err
	}

	return updatedID, nil
}

// GetUserDataByUuid は、UUIDに基づきユーザーデータと関連するクイズ/アクション結果を取得します
func (uc *userDataUsecase) GetUserDataByUuid(uuid string) (*UserDataResponse, error) {
	userData, err := uc.userDataRepo.FindByUuid(uuid)
	if err != nil {
		return nil, err
	}

	userQuizResults, err := uc.userQuizRepo.FindByUserDataID(int(userData.ID))
	if err != nil {
		return nil, err
	}
	quizCorrectRates := []QuizCorrectRate{}
	for _, result := range userQuizResults {
		quiz, err := uc.quizRepo.FindByID(result.QuizID)
		if err != nil {
			return nil, err
		}
		quizCorrectRates = append(quizCorrectRates, QuizCorrectRate{
			Name:        quiz.Name,
			CorrectRate: result.CorrectRate,
			Detail:      quiz.Detail,
		})
	}

	userActionResults, err := uc.userActionRepo.FindByUserDataID(int(userData.ID))
	if err != nil {
		return nil, err
	}
	actionCorrectRates := []ActionCorrectRate{}
	for _, result := range userActionResults {
		actionCorrectRates = append(actionCorrectRates, ActionCorrectRate{
			CorrectRate: result.CorrectRate,
		})
	}

	response := &UserDataResponse{
		Ratio:             userData.Ratio,
		Distance:          userData.Distance,
		ChangeCount:       userData.ChangeCount,
		QuizCorrectRates:  quizCorrectRates,
		ActionCorrectRate: actionCorrectRates[0].CorrectRate,
	}

	return response, nil
}

// SaveUserQuizResult はクイズ結果を保存または更新します
func (uc *userDataUsecase) SaveUserQuizResult(uuid string, quizID int, correct bool) error {
	userData, err := uc.userDataRepo.FindByUuid(uuid)
	if err != nil {
		return err
	}

	existingResult, err := uc.userQuizRepo.FindByUserDataAndQuizID(int(userData.ID), quizID)
	if err != nil {
		return err
	}

	correctRate := 0.0
	if correct {
		correctRate = 1.0
	}

	if existingResult != nil {
		existingResult.CorrectRate = (existingResult.CorrectRate*float64(existingResult.AttemptCount) + correctRate) / float64(existingResult.AttemptCount+1)
		existingResult.AttemptCount += 1
		return uc.userQuizRepo.Update(existingResult)
	}

	quizResult := models.UserQuizResult{
		UserDataID:   userData.ID,
		QuizID:       quizID,
		CorrectRate:  correctRate,
		AttemptCount: 1,
	}
	return uc.userQuizRepo.Save(quizResult)
}

// SaveUserActionResult はアクション結果を保存または更新します
func (uc *userDataUsecase) SaveUserActionResult(uuid string, actionID int, correct bool) error {
	userData, err := uc.userDataRepo.FindByUuid(uuid)
	if err != nil {
		return err
	}

	existingResult, err := uc.userActionRepo.FindByUserDataAndActionID(int(userData.ID), actionID)
	if err != nil {
		return err
	}

	correctRate := 0.0
	if correct {
		correctRate = 1.0
	}

	if existingResult != nil {
		existingResult.CorrectRate = (existingResult.CorrectRate*float64(existingResult.AttemptCount) + correctRate) / float64(existingResult.AttemptCount+1)
		existingResult.AttemptCount += 1
		return uc.userActionRepo.Update(existingResult)
	}

	actionResult := models.UserActionResult{
		UserDataID:   userData.ID,
		ActionID:     actionID,
		CorrectRate:  correctRate,
		AttemptCount: 1,
	}
	return uc.userActionRepo.Save(actionResult)
}

// UserDataResponse は GetUserDataByUuid で返されるレスポンス構造体
type UserDataResponse struct {
	Ratio             float64           `json:"ratio"`
	Distance          float64           `json:"distance"`
	ChangeCount       int               `json:"change_count"`
	QuizCorrectRates  []QuizCorrectRate `json:"quiz_correct_rates"`
	ActionCorrectRate float64           `json:"action_correct_rates"`
}

// QuizCorrectRate はその正解率、名前、詳細を保持する構造体
type QuizCorrectRate struct {
	Name        string  `json:"name"`
	CorrectRate float64 `json:"correct_rate"`
	Detail      string  `json:"detail"`
}

// ActionCorrectRate はその正解率を保持する構造体
type ActionCorrectRate struct {
	CorrectRate float64 `json:"correct_rate"`
}
