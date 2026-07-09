# 2026-07-09

## 증상

차량 선택 시 앱 종료

## Exception

SQLite Error 1: no such table: VehicleResourceFiles

## 원인 추정

EF migration 상태와 실제 SQLite DB 스키마가 불일치

## 해결

앱 시작 시 VehicleResourceFiles 테이블을 강제 생성