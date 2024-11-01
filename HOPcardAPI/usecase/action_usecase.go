package usecase

import (
	"HOPcardAPI/domain/models"
	"HOPcardAPI/domain/repositories"
)

type ActionUseCase interface {
	GetActionByID(id int) (*models.Action, error)
}

type actionUseCase struct {
	actionRepository repositories.ActionRepository
}

func NewActionUseCase(actionrepo repositories.ActionRepository) ActionUseCase {
	return &actionUseCase{
		actionRepository: actionrepo,
	}
}

func (uc *actionUseCase) GetActionByID(id int) (*models.Action, error) {
	action, err := uc.actionRepository.FindOneByID(id)
	if err != nil {
		return nil, err
	}
	return action, nil
}
