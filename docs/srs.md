# SRS (Software Requirements Specification)

# Rental CRM

Version: 0.1
Author: Antonio
Last Updated: 2026-07-06

---

# 1. Overview

## Purpose

본 프로그램은 장기렌트 상담 업무를 위한 고객 관리 시스템(CRM)이다.

현재 Word 문서와 Windows 폴더를 이용하여 상담 내용을 관리하는 방식을 하나의 프로그램으로 통합한다.

목표는 다음과 같다.

- 상담 기록 관리
- 고객 정보 관리
- 첨부파일 관리
- 빠른 검색
- 상담 이력 조회
- 계약 진행 상태 관리

---

# 2. Goals

사용자는 다음 작업을 30초 이내에 수행할 수 있어야 한다.

- 고객 검색
- 상담 내용 추가
- 견적서 첨부
- 면허증 확인
- 이전 상담 확인

---

# 3. Users

### 상담 직원

권한

- 고객 등록
- 고객 수정
- 상담기록 작성
- 첨부파일 업로드
- 검색

---

# 4. Customer

속성

- ID
- 이름
- 연락처
- 생년월일
- 주소 (선택)
- 직업 (선택)
- 회사명 (선택)
- 최초 문의일
- 최근 상담일
- 상태

상태

- 신규
- 상담중
- 심사진행
- 승인
- 계약완료
- 출고완료
- 종료
- 취소

---

# 5. Consultation Log

각 고객은 상담기록을 여러 개 가질 수 있다.

상담기록

- 작성일시
- 작성자
- 내용

예시

2026-07-06 14:30

"고객이 K5 프레스티지 문의.
월 예산 50만원.
배우자와 상의 후 연락 예정."

---

# 6. Attachments

고객은 여러 개의 첨부파일을 가질 수 있다.

지원 파일

- PDF
- JPG
- PNG
- DOCX
- XLSX

첨부파일 정보

- 파일명
- 저장경로
- 업로드일

실제 파일은 DB에 저장하지 않는다.

파일 구조

/files

    /10001
        quote.pdf
        license.jpg
        contract.pdf

---

# 7. Search

검색 가능

- 이름
- 연락처
- 차량명
- 상담상태

정렬

- 최근 상담순
- 최초 등록순
- 이름순

---

# 8. Dashboard

첫 화면

표시 정보

- 오늘 상담 예정
- 최근 등록 고객
- 계약 완료 건수
- 상담중 고객 수

---

# 9. Customer Detail

고객 상세 화면

좌측

- 고객정보

우측

- 상담기록(TimeLine)

하단

- 첨부파일

---

# 10. Functional Requirements

## 고객

- 고객 등록
- 고객 수정
- 고객 삭제

## 상담

- 상담기록 추가
- 상담기록 수정
- 상담기록 삭제

## 첨부파일

- Drag & Drop 업로드
- 다운로드
- 삭제

## 검색

- 실시간 검색

---

# 11. Non Functional Requirements

응답속도

- 검색 1초 이내

DB

- SQLite

플랫폼

- Windows

백업

- SQLite 파일 복사만으로 가능

첨부파일

- 로컬 폴더 저장

---

# 12. Database

Customer

- id
- name
- phone
- birthday
- status
- created_at
- updated_at

Consultation

- id
- customer_id
- created_at
- memo

Attachment

- id
- customer_id
- filename
- filepath
- uploaded_at

---

# 13. Future Features

예약 문자

계약 만기 알림

캘린더

차량 관리

심사 결과 관리

PDF 출력

엑셀 내보내기

모바일 대응

다중 사용자

로그인 기능

권한 관리
