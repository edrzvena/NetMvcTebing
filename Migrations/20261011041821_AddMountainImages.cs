using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tebing.Migrations
{
    /// <inheritdoc />
    public partial class AddMountainImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                table: "Mountains",
                newName: "ImagePath");

            migrationBuilder.AddColumn<string>(
                name: "ImageCredit",
                table: "Mountains",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageSourceUrl",
                table: "Mountains",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            // Isi foto contoh buat gunung seed yang udah ada di database (data sama dengan
            // Data/Seed/SeedMountainImages.cs, sengaja dibekukan di sini). Cuma jalan sekali bareng
            // migration ini; DB baru dapat fotonya langsung dari DbSeeder. Foto yang udah diisi admin gak ditimpa.
            migrationBuilder.Sql("UPDATE Mountains SET ImagePath = N'/images/mountains/gunung-gede', ImageCredit = N'Afrogindahood / CC BY-SA 3.0 / Wikimedia Commons', ImageSourceUrl = N'https://commons.wikimedia.org/wiki/File:Mount_Gede_01.jpg' WHERE Name = N'Gunung Gede' AND ImagePath IS NULL;");
            migrationBuilder.Sql("UPDATE Mountains SET ImagePath = N'/images/mountains/gunung-pangrango', ImageCredit = N'Herdik.herlambang / CC BY-SA 4.0 / Wikimedia Commons', ImageSourceUrl = N'https://commons.wikimedia.org/wiki/File:View_Mt_Gede_Pangrango_from_Mt_Gede_peak.jpg' WHERE Name = N'Gunung Pangrango' AND ImagePath IS NULL;");
            migrationBuilder.Sql("UPDATE Mountains SET ImagePath = N'/images/mountains/gunung-ciremai', ImageCredit = N'Achmad Hikam / CC BY-SA 4.0 / Wikimedia Commons', ImageSourceUrl = N'https://commons.wikimedia.org/wiki/File:View_of_Ciremai_Mount_from_Kuningan.jpg' WHERE Name = N'Gunung Ciremai' AND ImagePath IS NULL;");
            migrationBuilder.Sql("UPDATE Mountains SET ImagePath = N'/images/mountains/gunung-papandayan', ImageCredit = N'Hullie / CC BY-SA 2.5 / Wikimedia Commons', ImageSourceUrl = N'https://commons.wikimedia.org/wiki/File:Indonesia_-_papandayan_4.jpg' WHERE Name = N'Gunung Papandayan' AND ImagePath IS NULL;");
            migrationBuilder.Sql("UPDATE Mountains SET ImagePath = N'/images/mountains/gunung-cikuray', ImageCredit = N'Raffaelsatrya / CC0 / Wikimedia Commons', ImageSourceUrl = N'https://commons.wikimedia.org/wiki/File:Gunung_Cikuray_2025.jpg' WHERE Name = N'Gunung Cikuray' AND ImagePath IS NULL;");
            migrationBuilder.Sql("UPDATE Mountains SET ImagePath = N'/images/mountains/gunung-guntur', ImageCredit = N'Reyhansavero11 / CC BY-SA 4.0 / Wikimedia Commons', ImageSourceUrl = N'https://commons.wikimedia.org/wiki/File:Mount_Guntur_in_Western_Java,_Indonesia.jpg' WHERE Name = N'Gunung Guntur' AND ImagePath IS NULL;");
            migrationBuilder.Sql("UPDATE Mountains SET ImagePath = N'/images/mountains/gunung-salak', ImageCredit = N'Hiroshi sanjuro / CC BY-SA 4.0 / Wikimedia Commons', ImageSourceUrl = N'https://commons.wikimedia.org/wiki/File:Gunung_Salak_Dilihat_dari_Rainbow_Hill_Golf_Bogor.jpg' WHERE Name = N'Gunung Salak' AND ImagePath IS NULL;");
            migrationBuilder.Sql("UPDATE Mountains SET ImagePath = N'/images/mountains/gunung-burangrang', ImageCredit = N'Victory33 / CC BY 3.0 / Wikimedia Commons', ImageSourceUrl = N'https://commons.wikimedia.org/wiki/File:Gunung_burangrang_@_Villa_Alabare_-_panoramio.jpg' WHERE Name = N'Gunung Burangrang' AND ImagePath IS NULL;");
            migrationBuilder.Sql("UPDATE Mountains SET ImagePath = N'/images/mountains/gunung-manglayang', ImageCredit = N'Priskamarsila29 / CC BY-SA 4.0 / Wikimedia Commons', ImageSourceUrl = N'https://commons.wikimedia.org/wiki/File:Matahari_Terbit_di_Gunung_Manglayang.jpg' WHERE Name = N'Gunung Manglayang' AND ImagePath IS NULL;");
            migrationBuilder.Sql("UPDATE Mountains SET ImagePath = N'/images/mountains/gunung-prau', ImageCredit = N'Nanterjen / CC BY-SA 4.0 / Wikimedia Commons', ImageSourceUrl = N'https://commons.wikimedia.org/wiki/File:Mount_Parahu_in_the_Dieng_Plateau.jpg' WHERE Name = N'Gunung Prau' AND ImagePath IS NULL;");
            migrationBuilder.Sql("UPDATE Mountains SET ImagePath = N'/images/mountains/gunung-merbabu', ImageCredit = N'Adri45an / CC BY-SA 4.0 / Wikimedia Commons', ImageSourceUrl = N'https://commons.wikimedia.org/wiki/File:MtmerbabuA45d.jpg' WHERE Name = N'Gunung Merbabu' AND ImagePath IS NULL;");
            migrationBuilder.Sql("UPDATE Mountains SET ImagePath = N'/images/mountains/gunung-sindoro', ImageCredit = N'Chrisila Wentiasri / CC BY-SA 4.0 / Wikimedia Commons', ImageSourceUrl = N'https://commons.wikimedia.org/wiki/File:The_Peak_of_Sindoro_Mountain.jpg' WHERE Name = N'Gunung Sindoro' AND ImagePath IS NULL;");
            migrationBuilder.Sql("UPDATE Mountains SET ImagePath = N'/images/mountains/gunung-sumbing', ImageCredit = N'CEphoto, Uwe Aranas / CC BY-SA 3.0 / Wikimedia Commons', ImageSourceUrl = N'https://commons.wikimedia.org/wiki/File:Indonesia_Mount-Sumbing-volcano-01.jpg' WHERE Name = N'Gunung Sumbing' AND ImagePath IS NULL;");
            migrationBuilder.Sql("UPDATE Mountains SET ImagePath = N'/images/mountains/gunung-slamet', ImageCredit = N'Fajarbanyumas / CC BY-SA 4.0 / Wikimedia Commons', ImageSourceUrl = N'https://commons.wikimedia.org/wiki/File:Gunung_Slamer_dari_Limpakuwus_(1).jpg' WHERE Name = N'Gunung Slamet' AND ImagePath IS NULL;");
            migrationBuilder.Sql("UPDATE Mountains SET ImagePath = N'/images/mountains/gunung-lawu', ImageCredit = N'Ahmadafif1995 / CC BY-SA 4.0 / Wikimedia Commons', ImageSourceUrl = N'https://commons.wikimedia.org/wiki/File:Gunung_lawu.jpg' WHERE Name = N'Gunung Lawu' AND ImagePath IS NULL;");
            migrationBuilder.Sql("UPDATE Mountains SET ImagePath = N'/images/mountains/gunung-andong', ImageCredit = N'Hendrojkson / CC BY-SA 4.0 / Wikimedia Commons', ImageSourceUrl = N'https://commons.wikimedia.org/wiki/File:Pagi_Di_Puncak_Gunung_Andong.jpg' WHERE Name = N'Gunung Andong' AND ImagePath IS NULL;");
            migrationBuilder.Sql("UPDATE Mountains SET ImagePath = N'/images/mountains/gunung-ungaran', ImageCredit = N'Mrgostuquwh / CC BY-SA 4.0 / Wikimedia Commons', ImageSourceUrl = N'https://commons.wikimedia.org/wiki/File:Gunung_Ungaran.jpg' WHERE Name = N'Gunung Ungaran' AND ImagePath IS NULL;");
            migrationBuilder.Sql("UPDATE Mountains SET ImagePath = N'/images/mountains/gunung-merapi', ImageCredit = N'Fahmi Widayat / CC BY-SA 4.0 / Wikimedia Commons', ImageSourceUrl = N'https://commons.wikimedia.org/wiki/File:Pendidikan_Karakter_Dan_Nasionalisme.jpg' WHERE Name = N'Gunung Merapi' AND ImagePath IS NULL;");
            migrationBuilder.Sql("UPDATE Mountains SET ImagePath = N'/images/mountains/gunung-semeru', ImageCredit = N'Pambudiyoga / CC BY-SA 4.0 / Wikimedia Commons', ImageSourceUrl = N'https://commons.wikimedia.org/wiki/File:Mount_Semeru.jpg' WHERE Name = N'Gunung Semeru' AND ImagePath IS NULL;");
            migrationBuilder.Sql("UPDATE Mountains SET ImagePath = N'/images/mountains/gunung-arjuno', ImageCredit = N'Vhilicious / CC BY-SA 4.0 / Wikimedia Commons', ImageSourceUrl = N'https://commons.wikimedia.org/wiki/File:Mount_Arjuno,_Malang_Regency,_East_Java.jpg' WHERE Name = N'Gunung Arjuno' AND ImagePath IS NULL;");
            migrationBuilder.Sql("UPDATE Mountains SET ImagePath = N'/images/mountains/gunung-penanggungan', ImageCredit = N'Ivuvisual / CC BY-SA 4.0 / Wikimedia Commons', ImageSourceUrl = N'https://commons.wikimedia.org/wiki/File:Ritual_Metik_Padi.jpg' WHERE Name = N'Gunung Penanggungan' AND ImagePath IS NULL;");
            migrationBuilder.Sql("UPDATE Mountains SET ImagePath = N'/images/mountains/gunung-raung', ImageCredit = N'Adri45an / CC BY-SA 4.0 / Wikimedia Commons', ImageSourceUrl = N'https://commons.wikimedia.org/wiki/File:G.Raung%2713.jpg' WHERE Name = N'Gunung Raung' AND ImagePath IS NULL;");
            migrationBuilder.Sql("UPDATE Mountains SET ImagePath = N'/images/mountains/gunung-argopuro', ImageCredit = N'Snd estrada / CC BY-SA 4.0 / Wikimedia Commons', ImageSourceUrl = N'https://commons.wikimedia.org/wiki/File:Argpuro_(Danau_Taman_Hidup).jpg' WHERE Name = N'Gunung Argopuro' AND ImagePath IS NULL;");
            migrationBuilder.Sql("UPDATE Mountains SET ImagePath = N'/images/mountains/gunung-butak', ImageCredit = N'Nanok Kaesan / CC BY-SA 4.0 / Wikimedia Commons', ImageSourceUrl = N'https://commons.wikimedia.org/wiki/File:Pos_4_Gunung_butak_padang_sabana.jpg' WHERE Name = N'Gunung Butak' AND ImagePath IS NULL;");
            migrationBuilder.Sql("UPDATE Mountains SET ImagePath = N'/images/mountains/gunung-bromo', ImageCredit = N'Thomas Hirsch / CC BY-SA 3.0 / Wikimedia Commons', ImageSourceUrl = N'https://commons.wikimedia.org/wiki/File:Mount_Bromo_at_sunrise,_showing_its_volcanoes_and_Mount_Semeru_(background).jpg' WHERE Name = N'Gunung Bromo' AND ImagePath IS NULL;");
            migrationBuilder.Sql("UPDATE Mountains SET ImagePath = N'/images/mountains/gunung-agung', ImageCredit = N'Tiket2 / CC BY 2.0 / Wikimedia Commons', ImageSourceUrl = N'https://commons.wikimedia.org/wiki/File:Gunung_Agung,_an_active_volcano_in_Bali,_Indonesia;_January_2020.jpg' WHERE Name = N'Gunung Agung' AND ImagePath IS NULL;");
            migrationBuilder.Sql("UPDATE Mountains SET ImagePath = N'/images/mountains/gunung-batur', ImageCredit = N'Jnpet / CC BY-SA 3.0 / Wikimedia Commons', ImageSourceUrl = N'https://commons.wikimedia.org/wiki/File:Mount_Batur.JPG' WHERE Name = N'Gunung Batur' AND ImagePath IS NULL;");
            migrationBuilder.Sql("UPDATE Mountains SET ImagePath = N'/images/mountains/gunung-batukaru', ImageCredit = N'musnahterinjak / CC BY-SA 3.0 / Wikimedia Commons', ImageSourceUrl = N'https://commons.wikimedia.org/wiki/File:Gunung_Kembar_Batukaru_-_panoramio.jpg' WHERE Name = N'Gunung Batukaru' AND ImagePath IS NULL;");
            migrationBuilder.Sql("UPDATE Mountains SET ImagePath = N'/images/mountains/gunung-rinjani', ImageCredit = N'Suryanata budi / CC BY-SA 4.0 / Wikimedia Commons', ImageSourceUrl = N'https://commons.wikimedia.org/wiki/File:KAGAGAHAN_RIJANI.jpg' WHERE Name = N'Gunung Rinjani' AND ImagePath IS NULL;");
            migrationBuilder.Sql("UPDATE Mountains SET ImagePath = N'/images/mountains/gunung-tambora', ImageCredit = N'NASA Expedition 20 crew / Public domain / Wikimedia Commons', ImageSourceUrl = N'https://commons.wikimedia.org/wiki/File:Mount_Tambora_Volcano,_Sumbawa_Island,_Indonesia.jpg' WHERE Name = N'Gunung Tambora' AND ImagePath IS NULL;");
            migrationBuilder.Sql("UPDATE Mountains SET ImagePath = N'/images/mountains/gunung-mutis', ImageCredit = N'Lorensa Aldayanti Tanebeth / CC BY-SA 4.0 / Wikimedia Commons', ImageSourceUrl = N'https://commons.wikimedia.org/wiki/File:Dua_pohon_besar_yang_bisa_ditemui_di_dekat_pintu_masuk_lereng_gunung_mutis.jpg' WHERE Name = N'Gunung Mutis' AND ImagePath IS NULL;");
            migrationBuilder.Sql("UPDATE Mountains SET ImagePath = N'/images/mountains/gunung-kerinci', ImageCredit = N'Uprising / CC BY-SA 3.0 / Wikimedia Commons', ImageSourceUrl = N'https://commons.wikimedia.org/wiki/File:Uprising-mount_kerinci.jpg' WHERE Name = N'Gunung Kerinci' AND ImagePath IS NULL;");
            migrationBuilder.Sql("UPDATE Mountains SET ImagePath = N'/images/mountains/gunung-marapi', ImageCredit = N'Hanafiannoy / Public domain / Wikimedia Commons', ImageSourceUrl = N'https://commons.wikimedia.org/wiki/File:Marapi_and_Bukittinggi.jpg' WHERE Name = N'Gunung Marapi' AND ImagePath IS NULL;");
            migrationBuilder.Sql("UPDATE Mountains SET ImagePath = N'/images/mountains/gunung-singgalang', ImageCredit = N'Firzafp / CC BY-SA 4.0 / Wikimedia Commons', ImageSourceUrl = N'https://commons.wikimedia.org/wiki/File:Sianok_Canyon_and_Mount_Singgalang_(view_from_Lobang_Jepang)_-_Bukit_Tinggi,_West_Sumatra_(27_October_2020)_(1).jpg' WHERE Name = N'Gunung Singgalang' AND ImagePath IS NULL;");
            migrationBuilder.Sql("UPDATE Mountains SET ImagePath = N'/images/mountains/gunung-talang', ImageCredit = N'Anak Sago / CC BY 4.0 / Wikimedia Commons', ImageSourceUrl = N'https://commons.wikimedia.org/wiki/File:Gunung_Talang.jpg' WHERE Name = N'Gunung Talang' AND ImagePath IS NULL;");
            migrationBuilder.Sql("UPDATE Mountains SET ImagePath = N'/images/mountains/gunung-sibayak', ImageCredit = N'swifant / CC BY-SA 2.0 / Wikimedia Commons', ImageSourceUrl = N'https://commons.wikimedia.org/wiki/File:Gunung_Sibayak_2009_(5502394580).jpeg' WHERE Name = N'Gunung Sibayak' AND ImagePath IS NULL;");
            migrationBuilder.Sql("UPDATE Mountains SET ImagePath = N'/images/mountains/gunung-sinabung', ImageCredit = N'Kenrick95 / CC BY-SA 3.0 / Wikimedia Commons', ImageSourceUrl = N'https://commons.wikimedia.org/wiki/File:Sinabung-Gundaling-20100913.JPG' WHERE Name = N'Gunung Sinabung' AND ImagePath IS NULL;");
            migrationBuilder.Sql("UPDATE Mountains SET ImagePath = N'/images/mountains/gunung-dempo', ImageCredit = N'Febi andika valendra / CC BY-SA 4.0 / Wikimedia Commons', ImageSourceUrl = N'https://commons.wikimedia.org/wiki/File:Gunung_dempo_(cropped).jpg' WHERE Name = N'Gunung Dempo' AND ImagePath IS NULL;");
            migrationBuilder.Sql("UPDATE Mountains SET ImagePath = N'/images/mountains/gunung-kaba', ImageCredit = N'Kibe00 / CC BY 4.0 / Wikimedia Commons', ImageSourceUrl = N'https://commons.wikimedia.org/wiki/File:Kawah_mati_Bukit_Kaba.jpg' WHERE Name = N'Gunung Kaba' AND ImagePath IS NULL;");
            migrationBuilder.Sql("UPDATE Mountains SET ImagePath = N'/images/mountains/gunung-leuser', ImageCredit = N'gbohne / CC BY-SA 2.0 / Wikimedia Commons', ImageSourceUrl = N'https://commons.wikimedia.org/wiki/File:Sunrises_-_Mount_Leuser_National_Park.jpg' WHERE Name = N'Gunung Leuser' AND ImagePath IS NULL;");
            migrationBuilder.Sql("UPDATE Mountains SET ImagePath = N'/images/mountains/gunung-latimojong', ImageCredit = N'Hamka Rasufit / CC0 / Wikimedia Commons', ImageSourceUrl = N'https://commons.wikimedia.org/wiki/File:Misty_Morning_Mount_Latimojong.jpg' WHERE Name = N'Gunung Latimojong' AND ImagePath IS NULL;");
            migrationBuilder.Sql("UPDATE Mountains SET ImagePath = N'/images/mountains/gunung-binaiya', ImageCredit = N'Gekko Studio / CC BY 3.0 / Wikimedia Commons', ImageSourceUrl = N'https://commons.wikimedia.org/wiki/File:Mount_Binaiya.jpg' WHERE Name = N'Gunung Binaiya' AND ImagePath IS NULL;");
            migrationBuilder.Sql("UPDATE Mountains SET ImagePath = N'/images/mountains/puncak-jaya-carstensz', ImageCredit = N'Enda Kaban / CC BY-SA 4.0 / Wikimedia Commons', ImageSourceUrl = N'https://commons.wikimedia.org/wiki/File:Carstenzs_Piramida_Mountain.jpg' WHERE Name = N'Puncak Jaya (Carstensz)' AND ImagePath IS NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageCredit",
                table: "Mountains");

            migrationBuilder.DropColumn(
                name: "ImageSourceUrl",
                table: "Mountains");

            migrationBuilder.RenameColumn(
                name: "ImagePath",
                table: "Mountains",
                newName: "ImageUrl");
        }
    }
}
