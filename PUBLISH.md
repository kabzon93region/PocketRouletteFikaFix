# Publish to GitHub — PocketRoulette Fika Fix

**Статус:** `ready`  
**GitHub:** Release + zip  
**Версия:** `1.0.1`  
**Deployment:** `(headless_client,headless_host)`

## 1. Подготовка (уже сделано этим скриптом)

Папка: `github-repos/PocketRouletteFikaFix/`

## 2. Создать репозиторий и запушить

```powershell
cd github-repos/PocketRouletteFikaFix
git init
git add .
git commit -m "Source backup PocketRoulette Fika Fix v1.0.1"
git branch -M main
git remote add origin https://github.com/kabzon93region/PocketRouletteFikaFix.git
git push -u origin main
```

Или автоматически:

```powershell
python CURSORAIMODING/tools/publish/publish_github_release.py PocketRouletteFikaFix --create-repo
```

## 3. GitHub Release

Прикрепить zip (только игровые файлы, без INSTALL.md):

`\\Servant\data\Games\EscapeFromTarkov4\CURSORAIMODING\releases\PocketRouletteFikaFix_(headless_client,headless_host)_v1.0.1_2026-06-26.zip`

```powershell
gh release create v1.0.1 "\\Servant\data\Games\EscapeFromTarkov4\CURSORAIMODING\releases\PocketRouletteFikaFix_(headless_client,headless_host)_v1.0.1_2026-06-26.zip" ^
  --title "PocketRoulette Fika Fix v1.0.1" ^
  --notes-file CHANGELOG.md
```

## Описание репозитория (suggested)

Мост PocketRoulette между клиентом и headless-хостом.

SPT 4.0 + Fika 2.3 headless stack. Deployment: `(headless_client,headless_host)`.
