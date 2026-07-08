# Consult Note Deployment

## 목표

Consult Note는 Windows 단일 사용자 로컬 앱으로 배포한다.
사용자는 .NET SDK, Node.js, npm, Git 같은 개발 도구를 설치하지 않고 압축을 해제한 뒤 실행 파일만 실행할 수 있어야 한다.

## 기본 원칙

- WPF, SQLite, 로컬 단일 사용자 앱 구조를 유지한다.
- 사용자 데이터는 업데이트 대상에서 제외한다.
- 일반 사용자에게 Git 사용을 요구하지 않는다.
- 업데이트 실패 시 기존 데이터와 기존 실행 가능 상태가 유지되어야 한다.

## 런타임 폴더 구조

초기 배포본을 압축 해제하면 실행 폴더에는 다음 항목이 생기거나 생성될 수 있다.

```text
SalesConsultationTracker.exe
필요한 dll 및 런타임 파일
consultnote.db
storage/
backup/
logs/
settings/
```

`consultnote.db`, `storage/`, `backup/`, `logs/`, `settings/`는 앱 실행 폴더 기준 로컬 데이터 영역이다.

## 보존해야 하는 항목

업데이트 시 다음 항목은 삭제하거나 덮어쓰지 않는다.

- `consultnote.db`
- `storage/`
- `backup/`
- `logs/`
- `settings/`

특히 `consultnote.db`와 `storage/`는 고객 상담 데이터와 첨부 파일을 담으므로 반드시 보존한다.

## 업데이트 zip 제외 항목

업데이트용 zip에는 다음 항목을 포함하지 않는다.

- `consultnote.db`
- `storage/`
- `backup/`
- `logs/`
- `settings/`

업데이트 zip은 실행 파일과 앱 dll, 필요한 런타임 파일만 포함하는 것을 기본으로 한다.

## 초기 배포와 업데이트 배포

초기 배포:

- 새 PC나 새 폴더에 처음 설치할 때 사용한다.
- 압축 해제 후 `SalesConsultationTracker.exe`를 실행한다.
- 앱이 실행되면서 DB와 로컬 폴더를 생성한다.

업데이트 배포:

- 이미 사용 중인 실행 폴더에 새 앱 파일만 교체한다.
- 사용자 데이터 폴더와 DB는 유지한다.
- 업데이트 전 앱을 종료한 상태에서 적용한다.

## 현재 수동 배포 절차

개발 PC에서 다음 명령으로 self-contained zip을 생성한다.

```powershell
.\scripts\publish.ps1
```

생성 위치:

```text
dist/SalesConsultationTracker_yyyyMMdd_HHmm.zip
```

GitHub Release를 사용할 경우:

1. `scripts/publish.ps1`로 zip을 만든다.
2. GitHub Release를 생성한다.
3. 생성된 zip을 Release asset으로 업로드한다.
4. 사용자에게 Release zip 다운로드 링크를 전달한다.

## 앱 내 업데이트 확인

앱은 `업데이트 확인` 버튼으로 GitHub 최신 Release 버전을 확인할 수 있다.
새 버전이 있으면 GitHub Release 페이지를 열어 사용자가 zip을 직접 내려받는다.
현재 단계에서는 앱이 스스로 zip을 다운로드하거나 실행 파일을 교체하지 않는다.

## 향후 자동 업데이트 계획

실행 파일 자동 교체까지 포함하는 자동 업데이트는 P3 후순위 기능으로 둔다.
후보 방식은 GitHub Release와 AutoUpdater.NET 조합이다.

검토할 항목:

- 앱 버전과 Release 버전 매칭
- `update.xml` 구조
- 업데이트 zip 파일 목록
- 업데이트 실패 시 기존 버전 유지
- 업데이트 후 앱 재시작 동작
- 실행 중인 앱 파일 교체 방식
- GitHub Actions로 Release asset 자동 생성 가능성

## 현재 완료된 기반

- self-contained publish 스크립트
- zip 생성 자동화
- 앱 화면 버전 표시
- 앱 내 GitHub Release 최신 버전 확인 버튼
- 사용자 데이터와 앱 파일 분리 원칙 문서화
