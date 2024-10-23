include .env
export

MIGRATE := migrate -database "${DATABASE_URL}"

# マイグレーションファイルの生成
.PHONY: migrate-create
migrate-create:
	@read -p "Enter migration name: " name; \
	migrate create -ext sql -dir db/migrations -seq $$name

# マイグレーションを実行（アップ）
.PHONY: migrate-up
migrate-up:
	$(MIGRATE) -path db/migrations up

# マイグレーションを1つ戻す（ダウン）
.PHONY: migrate-down
migrate-down:
	$(MIGRATE) -path db/migrations down 1

# マイグレーションを全て戻す
.PHONY: migrate-reset
migrate-reset:
	$(MIGRATE) -path db/migrations down

# 現在のバージョンを確認
.PHONY: migrate-version
migrate-version:
	$(MIGRATE) -path db/migrations version

# Dirtyフラグを強制的にクリア
.PHONY: migrate-force
migrate-force:
	@read -p "Enter version number to force: " version; \
	$(MIGRATE) -path db/migrations force $$version

# マイグレーション状態を確認
.PHONY: migrate-status
migrate-status:
	$(MIGRATE) -path db/migrations status