using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ConsultNote.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddVehicleFuelTypesAndSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FuelTypes",
                table: "Vehicles",
                type: "TEXT",
                maxLength: 120,
                nullable: true);

            migrationBuilder.InsertData(
                table: "Vehicles",
                columns: new[] { "Id", "Brand", "CreatedAt", "FuelTypes", "IsActive", "Memo", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "현대", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc), "가솔린", true, "경차", "캐스퍼", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, "기아", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc), "가솔린, 전기", true, "경차", "레이", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 3, "기아", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc), "가솔린", true, "경차", "모닝", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 4, "현대", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc), "전기", true, "소형", "캐스퍼", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 5, "현대", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc), "가솔린, LPG", true, "준중형", "아반떼", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 6, "현대", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc), "가솔린", true, "준중형", "아반떼N", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 7, "기아", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc), "전기", true, "준중형", "EV4", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 8, "현대", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc), "가솔린, 하이브리드, LPG", true, "중형", "쏘나타", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 9, "현대", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc), "전기", true, "중형", "아이오닉6", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 10, "현대", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc), "전기", true, "중형", "아이오닉6N", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 11, "기아", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc), "가솔린, 하이브리드, LPG", true, "중형", "K5", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 12, "제네시스", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc), "가솔린", true, "중형", "G70", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 13, "현대", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc), "가솔린, 하이브리드, LPG", true, "준대형", "그랜저", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 14, "기아", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc), "가솔린, 하이브리드, LPG", true, "준대형", "K8", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 15, "제네시스", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc), "가솔린", true, "준대형", "G80", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 16, "제네시스", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc), "전기", true, "준대형", "G80 electric", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 17, "기아", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc), "가솔린", true, "대형", "K9", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 18, "제네시스", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc), "가솔린", true, "대형", "G90", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 19, "현대", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc), "가솔린", true, "소형SUV", "코나", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 20, "현대", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc), "전기", true, "소형SUV", "코나EV", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 21, "현대", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc), "가솔린", true, "소형SUV", "베뉴", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 22, "기아", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc), "가솔린, 하이브리드", true, "소형SUV", "셀토스", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 23, "기아", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc), "하이브리드", true, "소형SUV", "니로", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 24, "기아", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc), "전기", true, "소형SUV", "EV3", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 25, "쉐보레", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "소형SUV", "트렉스 크로스오버", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 26, "쉐보레", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "소형SUV", "트레일블레이저", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 27, "KGM", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "소형SUV", "티볼리", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 28, "르노코리아", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "소형SUV", "아르카나", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 29, "현대", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc), "하이브리드, 가솔린", true, "준중형SUV / 5인", "투싼", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 30, "기아", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc), "가솔린, 하이브리드, LPG", true, "준중형SUV / 5인", "스포티지", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 31, "기아", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc), "전기", true, "준중형SUV / 5인", "EV5", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 32, "기아", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc), "전기", true, "준중형SUV / 5인", "EV6", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 33, "제네시스", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc), "전기", true, "준중형SUV / 5인", "GV60", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 34, "쉐보레", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "준중형SUV / 5인", "이쿼녹스", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 35, "KGM", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "준중형SUV / 5인", "액티언", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 36, "KGM", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "준중형SUV / 5인", "토레스", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 37, "현대", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc), "하이브리드, 가솔린", true, "중형SUV / 5, 6, 7인승", "싼타페", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 38, "현대", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc), "전기", true, "중형SUV / 5, 6, 7인승", "아이오닉5", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 39, "현대", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc), "전기", true, "중형SUV / 5, 6, 7인승", "아이오닉5N", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 40, "기아", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc), "하이브리드, 가솔린, 디젤", true, "중형SUV / 5, 6, 7인승", "쏘렌토", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 41, "제네시스", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc), "가솔린", true, "중형SUV / 5, 6, 7인승", "GV70", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 42, "제네시스", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc), "전기", true, "중형SUV / 5, 6, 7인승", "GV70 electric", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 43, "KGM", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "중형SUV / 5, 6, 7인승", "렉스턴", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 44, "르노코리아", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "중형SUV / 5, 6, 7인승", "그랑 콜레오스", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 45, "현대", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc), "가솔린, 하이브리드", true, "대형SUV / 내연: 7, 9인승 / 전기: 6, 7인승", "팰리세이드", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 46, "현대", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc), "전기", true, "대형SUV / 내연: 7, 9인승 / 전기: 6, 7인승", "아이오닉9", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 47, "기아", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc), "전기", true, "대형SUV / 내연: 7, 9인승 / 전기: 6, 7인승", "EV9", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 48, "제네시스", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc), "가솔린", true, "대형SUV / 5, 6, 7인승", "GV80", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 49, "르노코리아", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "대형SUV / 내연: 7, 9인승 / 전기: 6, 7인승", "필랑트", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 50, "현대", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc), "가솔린, 하이브리드, LPG", true, "RV/MPV(승합차) / 좌석 옵션 다양함", "스타리아", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 51, "현대", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc), "전기", true, "RV/MPV(승합차)", "스타리아 일렉트릭", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 52, "기아", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc), "가솔린, 하이브리드", true, "RV/MPV(승합차) / 7, 9인승", "카니발", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 53, "기아", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc), "전기", true, "RV/MPV(승합차) / 5인승", "PV5", new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc) }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: 27);

            migrationBuilder.DeleteData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: 28);

            migrationBuilder.DeleteData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: 29);

            migrationBuilder.DeleteData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: 30);

            migrationBuilder.DeleteData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: 31);

            migrationBuilder.DeleteData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: 32);

            migrationBuilder.DeleteData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: 33);

            migrationBuilder.DeleteData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: 34);

            migrationBuilder.DeleteData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: 35);

            migrationBuilder.DeleteData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: 36);

            migrationBuilder.DeleteData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: 37);

            migrationBuilder.DeleteData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: 38);

            migrationBuilder.DeleteData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: 39);

            migrationBuilder.DeleteData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: 40);

            migrationBuilder.DeleteData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: 41);

            migrationBuilder.DeleteData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: 42);

            migrationBuilder.DeleteData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: 43);

            migrationBuilder.DeleteData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: 44);

            migrationBuilder.DeleteData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: 45);

            migrationBuilder.DeleteData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: 46);

            migrationBuilder.DeleteData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: 47);

            migrationBuilder.DeleteData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: 48);

            migrationBuilder.DeleteData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: 49);

            migrationBuilder.DeleteData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: 50);

            migrationBuilder.DeleteData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: 51);

            migrationBuilder.DeleteData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: 52);

            migrationBuilder.DeleteData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: 53);

            migrationBuilder.DropColumn(
                name: "FuelTypes",
                table: "Vehicles");
        }
    }
}
