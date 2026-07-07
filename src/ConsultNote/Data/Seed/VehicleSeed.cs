using ConsultNote.Data.Entities;

namespace ConsultNote.Data.Seed;

public static class VehicleSeed
{
    private static readonly DateTime SeededAt = new(2026, 7, 7, 0, 0, 0, DateTimeKind.Utc);

    public static readonly Vehicle[] Items =
    [
        Vehicle(1, "현대", "캐스퍼", "가솔린", "경차"),
        Vehicle(2, "기아", "레이", "가솔린, 전기", "경차"),
        Vehicle(3, "기아", "모닝", "가솔린", "경차"),
        Vehicle(4, "현대", "캐스퍼", "전기", "소형"),
        Vehicle(5, "현대", "아반떼", "가솔린, LPG", "준중형"),
        Vehicle(6, "현대", "아반떼N", "가솔린", "준중형"),
        Vehicle(7, "기아", "EV4", "전기", "준중형"),
        Vehicle(8, "현대", "쏘나타", "가솔린, 하이브리드, LPG", "중형"),
        Vehicle(9, "현대", "아이오닉6", "전기", "중형"),
        Vehicle(10, "현대", "아이오닉6N", "전기", "중형"),
        Vehicle(11, "기아", "K5", "가솔린, 하이브리드, LPG", "중형"),
        Vehicle(12, "제네시스", "G70", "가솔린", "중형"),
        Vehicle(13, "현대", "그랜저", "가솔린, 하이브리드, LPG", "준대형"),
        Vehicle(14, "기아", "K8", "가솔린, 하이브리드, LPG", "준대형"),
        Vehicle(15, "제네시스", "G80", "가솔린", "준대형"),
        Vehicle(16, "제네시스", "G80 electric", "전기", "준대형"),
        Vehicle(17, "기아", "K9", "가솔린", "대형"),
        Vehicle(18, "제네시스", "G90", "가솔린", "대형"),
        Vehicle(19, "현대", "코나", "가솔린", "소형SUV"),
        Vehicle(20, "현대", "코나EV", "전기", "소형SUV"),
        Vehicle(21, "현대", "베뉴", "가솔린", "소형SUV"),
        Vehicle(22, "기아", "셀토스", "가솔린, 하이브리드", "소형SUV"),
        Vehicle(23, "기아", "니로", "하이브리드", "소형SUV"),
        Vehicle(24, "기아", "EV3", "전기", "소형SUV"),
        Vehicle(25, "쉐보레", "트렉스 크로스오버", null, "소형SUV"),
        Vehicle(26, "쉐보레", "트레일블레이저", null, "소형SUV"),
        Vehicle(27, "KGM", "티볼리", null, "소형SUV"),
        Vehicle(28, "르노코리아", "아르카나", null, "소형SUV"),
        Vehicle(29, "현대", "투싼", "하이브리드, 가솔린", "준중형SUV / 5인"),
        Vehicle(30, "기아", "스포티지", "가솔린, 하이브리드, LPG", "준중형SUV / 5인"),
        Vehicle(31, "기아", "EV5", "전기", "준중형SUV / 5인"),
        Vehicle(32, "기아", "EV6", "전기", "준중형SUV / 5인"),
        Vehicle(33, "제네시스", "GV60", "전기", "준중형SUV / 5인"),
        Vehicle(34, "쉐보레", "이쿼녹스", null, "준중형SUV / 5인"),
        Vehicle(35, "KGM", "액티언", null, "준중형SUV / 5인"),
        Vehicle(36, "KGM", "토레스", null, "준중형SUV / 5인"),
        Vehicle(37, "현대", "싼타페", "하이브리드, 가솔린", "중형SUV / 5, 6, 7인승"),
        Vehicle(38, "현대", "아이오닉5", "전기", "중형SUV / 5, 6, 7인승"),
        Vehicle(39, "현대", "아이오닉5N", "전기", "중형SUV / 5, 6, 7인승"),
        Vehicle(40, "기아", "쏘렌토", "하이브리드, 가솔린, 디젤", "중형SUV / 5, 6, 7인승"),
        Vehicle(41, "제네시스", "GV70", "가솔린", "중형SUV / 5, 6, 7인승"),
        Vehicle(42, "제네시스", "GV70 electric", "전기", "중형SUV / 5, 6, 7인승"),
        Vehicle(43, "KGM", "렉스턴", null, "중형SUV / 5, 6, 7인승"),
        Vehicle(44, "르노코리아", "그랑 콜레오스", null, "중형SUV / 5, 6, 7인승"),
        Vehicle(45, "현대", "팰리세이드", "가솔린, 하이브리드", "대형SUV / 내연: 7, 9인승 / 전기: 6, 7인승"),
        Vehicle(46, "현대", "아이오닉9", "전기", "대형SUV / 내연: 7, 9인승 / 전기: 6, 7인승"),
        Vehicle(47, "기아", "EV9", "전기", "대형SUV / 내연: 7, 9인승 / 전기: 6, 7인승"),
        Vehicle(48, "제네시스", "GV80", "가솔린", "대형SUV / 5, 6, 7인승"),
        Vehicle(49, "르노코리아", "필랑트", null, "대형SUV / 내연: 7, 9인승 / 전기: 6, 7인승"),
        Vehicle(50, "현대", "스타리아", "가솔린, 하이브리드, LPG", "RV/MPV(승합차) / 좌석 옵션 다양함"),
        Vehicle(51, "현대", "스타리아 일렉트릭", "전기", "RV/MPV(승합차)"),
        Vehicle(52, "기아", "카니발", "가솔린, 하이브리드", "RV/MPV(승합차) / 7, 9인승"),
        Vehicle(53, "기아", "PV5", "전기", "RV/MPV(승합차) / 5인승"),
    ];

    private static Vehicle Vehicle(int id, string brand, string name, string? fuelTypes, string memo)
    {
        return new Vehicle
        {
            Id = id,
            Brand = brand,
            Name = name,
            FuelTypes = fuelTypes,
            Memo = memo,
            IsActive = true,
            CreatedAt = SeededAt,
            UpdatedAt = SeededAt,
        };
    }
}
