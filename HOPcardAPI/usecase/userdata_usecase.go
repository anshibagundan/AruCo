package usecase

import (
	"HOPcardAPI/domain/models"
	"HOPcardAPI/domain/repositories"
)

type UserDataUsecase struct {
	repo repositories.UserDataRepository
}

func NewUserDataUsecase(repo repositories.UserDataRepository) *UserDataUsecase {
	return &UserDataUsecase{repo}
}

func (uc *UserDataUsecase) SaveUserData(uuid string, ratio float64, distance float64) (string, error) {
	userData, err := uc.repo.FindByUuid(uuid)
	if err != nil {
		// データが存在しない場合は新規作成
		return uc.repo.Save(uuid, ratio, distance)
	}

	// データが存在する場合の処理
	// TODO:計算過程合ってるかチェック
	newRatio := (userData.Ratio + float64(userData.ChangeCount+1)*ratio) / float64(userData.ChangeCount+2)
	newDistance := userData.Distance + distance
	newChangeCount := userData.ChangeCount + 1

	userData.Ratio = newRatio
	userData.Distance = newDistance
	userData.ChangeCount = newChangeCount

	// 更新処理を行う
	return uc.repo.Update(userData)
}

func (uc *UserDataUsecase) GetUserDataByUuid(uuid string) (*models.UserData, error) {
	return uc.repo.FindByUuid(uuid)
}
