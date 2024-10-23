// domain/services/uuid_service.go
package services

import (
	"crypto/sha256"
	"encoding/hex"
	"github.com/google/uuid"
	"strconv"
	"time"
)

type UUIDService interface {
	GenerateUUID(code int) string
}

type uuidService struct{}

func NewUUIDService() UUIDService {
	return &uuidService{}
}

func (s *uuidService) GenerateUUID(code int) string {
	// 現在時刻とコードを組み合わせて一意性を確保
	seed := strconv.Itoa(code) + time.Now().String()

	// SHA-256ハッシュを生成
	hasher := sha256.New()
	hasher.Write([]byte(seed))
	hash := hex.EncodeToString(hasher.Sum(nil))

	// UUIDv5を生成 (SHA-1ベース)
	namespace := uuid.MustParse("6ba7b810-9dad-11d1-80b4-00c04fd430c8")
	uid := uuid.NewSHA1(namespace, []byte(hash))

	return uid.String()
}
