// main.go
package main

import (
	"HOPcardAPI/domain/services"
	"HOPcardAPI/infrastructure/persistence"
	"HOPcardAPI/interfaces/handlers"
	"HOPcardAPI/usecase"
	"fmt"
	"github.com/gorilla/mux"
	"github.com/joho/godotenv"
	"gorm.io/driver/postgres"
	"gorm.io/gorm"
	"log"
	"net/http"
	"os"
)

func main() {
	// データベース接続の初期化
	db, err := initDB()
	if err != nil {
		log.Fatal(err)
	}

	// 初期化はinfra(persistence)->domain/service->usecase->handlerの順番で行うようにしよう
	// uuid系の初期化
	uuidRepo := persistence.NewUUIDRepository(db)
	uuidService := services.NewUUIDService()
	uuidUseCase := usecase.NewUUIDUseCase(uuidRepo, uuidService)
	uuidHandler := handlers.NewUUIDHandler(uuidUseCase)

	//quiz,action系の初期化
	quizRepo := persistence.NewQuizRepository(db)
	actionRepo := persistence.NewActionRepository(db)

	//difficulty系の初期化
	difficultyUsecase := usecase.NewGameUsecase(quizRepo, actionRepo)
	difficultyHandler := handlers.NewWebSocketHandler(difficultyUsecase)

	// 他の初期化ここに書いてね

	// ルーティング
	r := mux.NewRouter()
	r.HandleFunc("/createuuid", uuidHandler.CreateUUID).Methods("POST")
	r.HandleFunc("/getuuid", uuidHandler.GetUUID).Methods("GET")
	r.HandleFunc("/ws/android/{uuid}", difficultyHandler.HandleAndroidWebSocket)
	r.HandleFunc("/ws/unity/{uuid}", difficultyHandler.HandleUnityWebSocket)

	log.Fatal(http.ListenAndServe(":8080", r))
}

// initDBは別ファイルの方がいいのかな\(´ω` \)
func initDB() (*gorm.DB, error) {
	// .envファイルの読み込み
	if err := godotenv.Load(); err != nil {
		log.Printf("Warning: .env file not found")
	}

	// 環境変数から接続情報を取得
	dbUser := os.Getenv("DB_USER")
	dbPassword := os.Getenv("DB_PASSWORD")
	dbHost := os.Getenv("DB_HOST")
	dbPort := os.Getenv("DB_PORT")
	dbName := os.Getenv("DB_NAME")

	// 接続文字列の構築
	dsn := fmt.Sprintf(
		"host=%s user=%s password=%s dbname=%s port=%s sslmode=require",
		dbHost, dbUser, dbPassword, dbName, dbPort,
	)

	// データベースに接続
	db, err := gorm.Open(postgres.Open(dsn), &gorm.Config{})
	if err != nil {
		return nil, fmt.Errorf("failed to connect database: %w", err)
	}

	return db, nil
}
