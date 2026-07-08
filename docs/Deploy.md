현재 목표:
Consult Note에 자동 업데이트 기반을 추가한다.
단, 실제 자동 업데이트 완성까지 하지 말고, 배포/업데이트 설계와 TODO 정리부터 한다.

반드시 지킬 것:
- WPF 유지
- SQLite 유지
- 로컬 단일 사용자 앱 유지
- 로그인/서버/웹앱 전환 금지
- 사용자 데이터 consultnote.db, storage/ 는 업데이트 대상에서 제외
- Git을 일반 사용자에게 요구하지 않음
- Node.js, npm, .NET SDK 설치 요구 금지

작업 범위:
1. docs/SRS.md를 읽고 현재 프로젝트 목적을 이해한다.
2. docs/TODO.md에 Auto Update 관련 항목을 추가한다.
3. P2 또는 P3에 Distribution / Auto Update 섹션을 만든다.
4. 아래 항목을 체크박스로 추가한다.

추가할 TODO:
- [ ] 앱 버전 표시
- [ ] GitHub Release 기반 배포 방식 검토
- [ ] AutoUpdater.NET 적용 검토
- [ ] update.xml 구조 정의
- [ ] 업데이트 zip에 포함할 파일 목록 정의
- [ ] 업데이트 zip에서 consultnote.db 제외
- [ ] 업데이트 zip에서 storage/ 제외
- [ ] 업데이트 zip에서 backup/ 제외
- [ ] 프로그램 내 "업데이트 확인" 버튼 추가
- [ ] 실행 시 업데이트 확인 옵션 추가
- [ ] 업데이트 실패 시 기존 버전 유지 확인
- [ ] 업데이트 후 앱 재시작 동작 확인
- [ ] 배포용 압축 파일 생성 절차 문서화
- [ ] GitHub Actions로 Release 자동 생성 가능성 검토

5. docs/DEPLOYMENT.md 파일을 새로 만든다.
6. DEPLOYMENT.md에는 다음 내용을 정리한다.

DEPLOYMENT.md에 포함할 내용:
- 현재 배포 목표
  - 압축 해제 후 ConsultNote.exe 실행
  - 개발 도구 설치 없이 실행
- 배포 폴더 구조
  - ConsultNote.exe
  - 필요한 dll
  - consultnote.db
  - storage/
  - backup/
  - logs/
- 업데이트 시 보존해야 하는 파일/폴더
  - consultnote.db
  - storage/
  - backup/
- 업데이트 zip에 포함하면 안 되는 항목
  - consultnote.db
  - storage/
  - backup/
- 초기 배포와 업데이트 배포의 차이
- GitHub Release를 통한 수동 배포 절차
- 향후 AutoUpdater.NET 적용 계획
- 향후 GitHub Actions 자동화 계획

주의:
이번 작업에서는 AutoUpdater.NET 패키지를 설치하거나 코드를 구현하지 않는다.
문서와 TODO 정리만 수행한다.
작업 후 변경된 파일 목록을 요약해줘.