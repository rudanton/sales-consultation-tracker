"""Export the repository user manual as a Korean, searchable PDF."""

import html
import re
from pathlib import Path

from reportlab.lib import colors
from reportlab.lib.pagesizes import A4
from reportlab.lib.styles import ParagraphStyle, getSampleStyleSheet
from reportlab.pdfbase import pdfmetrics
from reportlab.pdfbase.ttfonts import TTFont
from reportlab.platypus import (
    BaseDocTemplate, Frame, PageBreak, PageTemplate, Paragraph,
    Preformatted, Spacer, Table, TableStyle,
)


ROOT = Path(__file__).resolve().parents[1]
SOURCE = ROOT / "docs" / "사용설명서.md"
OUTPUT = ROOT / "output" / "pdf" / "ConsultNote_사용설명서.pdf"
FONT_DIR = Path("C:/Windows/Fonts")
pdfmetrics.registerFont(TTFont("Malgun", str(FONT_DIR / "malgun.ttf")))
pdfmetrics.registerFont(TTFont("MalgunBold", str(FONT_DIR / "malgunbd.ttf")))
pdfmetrics.registerFontFamily("Malgun", normal="Malgun", bold="MalgunBold", italic="Malgun", boldItalic="MalgunBold")

INK = colors.HexColor("#18304E")
MUTED = colors.HexColor("#4E6072")
LINE = colors.HexColor("#D2DBE2")
PALE = colors.HexColor("#EEF2F5")
styles = getSampleStyleSheet()
styles.add(ParagraphStyle("ManualBody", fontName="Malgun", fontSize=9.5, leading=16,
                          textColor=INK, spaceAfter=9, wordWrap="CJK"))
styles.add(ParagraphStyle("ManualH2", parent=styles["ManualBody"], fontName="MalgunBold",
                          fontSize=15, leading=23, spaceBefore=18, spaceAfter=11, keepWithNext=True))
styles.add(ParagraphStyle("ManualH3", parent=styles["ManualBody"], fontName="MalgunBold",
                          fontSize=11, leading=18, spaceBefore=10, spaceAfter=6, keepWithNext=True))
styles.add(ParagraphStyle("ManualList", parent=styles["ManualBody"], leftIndent=14,
                          firstLineIndent=-12, spaceAfter=5))
styles.add(ParagraphStyle("ManualCell", parent=styles["ManualBody"], fontSize=8.6, leading=14, spaceAfter=0))
styles.add(ParagraphStyle("ManualCode", parent=styles["ManualBody"], fontSize=8.3, leading=14,
                          leftIndent=8, rightIndent=8, backColor=PALE, borderPadding=9))
styles.add(ParagraphStyle("ManualTitle", parent=styles["ManualBody"], fontName="MalgunBold",
                          fontSize=30, leading=45))


def inline(text):
    text = html.escape(text)
    text = re.sub(r"\[([^\]]+)\]\([^)]*\)", r"\1", text)
    text = re.sub(r"\*\*(.+?)\*\*", r"<b>\1</b>", text)
    text = re.sub(r"`([^`]+)`", r'<font color="#355B7A">\1</font>', text)
    return text


class ManualDocument(BaseDocTemplate):
    def __init__(self, filename):
        super().__init__(str(filename), pagesize=A4, leftMargin=46, rightMargin=46,
                         topMargin=54, bottomMargin=48, title="Consult Note 사용 설명서",
                         author="Consult Note", subject="고객, 상담, 견적 관리 사용 안내")
        frame = Frame(self.leftMargin, self.bottomMargin, self.width, self.height,
                      leftPadding=0, rightPadding=0, topPadding=0, bottomPadding=0)
        self.addPageTemplates(PageTemplate(id="manual", frames=frame, onPage=self.decorate))

    def decorate(self, canvas, doc):
        canvas.saveState()
        width, height = A4
        canvas.setStrokeColor(LINE)
        canvas.setLineWidth(0.5)
        if doc.page > 1:
            canvas.line(46, height - 34, width - 46, height - 34)
            canvas.setFillColor(MUTED)
            canvas.setFont("Malgun", 8)
            canvas.drawString(46, height - 26, "Consult Note | 사용 설명서")
        canvas.line(46, 34, width - 46, 34)
        canvas.setFont("Malgun", 8)
        canvas.setFillColor(MUTED)
        canvas.drawString(46, 22, "2026-10-29 기준")
        canvas.drawRightString(width - 46, 22, str(doc.page))
        canvas.restoreState()

    def afterFlowable(self, flowable):
        if isinstance(flowable, Paragraph) and flowable.style.name == "ManualH2":
            title = flowable.getPlainText()
            if re.match(r"^\d+\.", title):
                key = "section-" + title.split(".", 1)[0]
                self.canv.bookmarkPage(key)
                self.canv.addOutlineEntry(title, key, level=0)


def table_rows(lines, start):
    rows = []
    while start < len(lines) and lines[start].startswith("|"):
        cells = [cell.strip() for cell in lines[start].strip().strip("|").split("|")]
        if not all(re.fullmatch(r":?-+:?", cell) for cell in cells):
            rows.append(cells)
        start += 1
    return rows, start


def create_pdf():
    lines = SOURCE.read_text(encoding="utf-8-sig").splitlines()
    story = [Paragraph("Consult Note", styles["ManualTitle"]),
             Paragraph("사용 가이드", styles["ManualH3"]), Spacer(1, 8)]
    i = 1
    while i < len(lines):
        line = lines[i].strip()
        if not line:
            i += 1
            continue
        if line == "## 목차":
            i += 1
            while i < len(lines) and not lines[i].startswith("## "):
                i += 1
            continue
        if line.startswith("## "):
            if re.match(r"## (4|7)\.", line):
                story.append(PageBreak())
            story.append(Paragraph(inline(line[3:]), styles["ManualH2"]))
            i += 1
        elif line.startswith("### "):
            story.append(Paragraph(inline(line[4:]), styles["ManualH3"]))
            i += 1
        elif line.startswith("```"):
            i += 1
            block = []
            while i < len(lines) and not lines[i].startswith("```"):
                block.append(lines[i])
                i += 1
            story.extend([Preformatted("\n".join(block), styles["ManualCode"]), Spacer(1, 9)])
            i += 1
        elif line.startswith("|"):
            rows, i = table_rows(lines, i)
            col_count = len(rows[0])
            proportions = [0.27, 0.73] if col_count == 2 else [0.23, 0.32, 0.45]
            data = [[Paragraph(inline(cell), styles["ManualCell"]) for cell in row] for row in rows]
            grid = Table(data, colWidths=[(A4[0] - 92) * x for x in proportions], repeatRows=1, hAlign="LEFT")
            grid.setStyle(TableStyle([
                ("BACKGROUND", (0, 0), (-1, 0), colors.HexColor("#D9E3EA")),
                ("ROWBACKGROUNDS", (0, 1), (-1, -1), [colors.HexColor("#F7F9FA"), colors.HexColor("#EEF2F5")]),
                ("VALIGN", (0, 0), (-1, -1), "TOP"),
                ("LEFTPADDING", (0, 0), (-1, -1), 8), ("RIGHTPADDING", (0, 0), (-1, -1), 8),
                ("TOPPADDING", (0, 0), (-1, -1), 7), ("BOTTOMPADDING", (0, 0), (-1, -1), 7),
                ("LINEBELOW", (0, 0), (-1, 0), 0.6, LINE),
                ("LINEBELOW", (0, 1), (-1, -1), 0.25, LINE),
            ]))
            story.extend([grid, Spacer(1, 12)])
        elif re.match(r"^(\d+\.\s|[-] )", line):
            story.append(Paragraph(inline(line), styles["ManualList"]))
            i += 1
        else:
            paragraph = [line]
            i += 1
            while i < len(lines) and lines[i].strip() and not re.match(r"^(#|\||```|\d+\.\s|[-] )", lines[i]):
                paragraph.append(lines[i].strip())
                i += 1
            story.append(Paragraph(inline(" ".join(paragraph)), styles["ManualBody"]))
    OUTPUT.parent.mkdir(parents=True, exist_ok=True)
    ManualDocument(OUTPUT).build(story)
    print(OUTPUT)


if __name__ == "__main__":
    create_pdf()
