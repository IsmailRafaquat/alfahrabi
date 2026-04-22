import * as ExcelJS from 'exceljs/dist/exceljs.min.js';
import {
  StaffSalaryGroupedRow,
  getStaffSalaryMonthDetails,
  getStaffSalaryMaxRows,
  getStaffSalaryOverallMonthTotal,
} from './salary-report.helper';

export interface StaffSalaryReportExcelExportInput {
  sheetName: string;
  monthKeys: string[];
  monthLabels: string[];
  data: StaffSalaryGroupedRow[];
}

export class StaffSalaryReportExcelJsExporter {
  static async buildWorkbook(input: StaffSalaryReportExcelExportInput): Promise<ExcelJS.Workbook> {
    const wb = new ExcelJS.Workbook();
    const ws = wb.addWorksheet(input.sheetName);

    const monthCount = input.monthKeys.length;
    const totalColumns = 1 + monthCount * 2;

    const dynamicColumns: Array<{ key: string; width: number }> = [];

    for (let i = 0; i < input.monthKeys.length; i++) {
      dynamicColumns.push({ key: `date_${i}`, width: 16 });
      dynamicColumns.push({ key: `salary_${i}`, width: 14 });
    }

    ws.columns = [{ key: 'staff', width: 24 }, ...dynamicColumns];

    const faintBorder: ExcelJS.Borders = {
      top: { style: 'hair', color: { argb: 'D9D9D9' } },
      left: { style: 'hair', color: { argb: 'D9D9D9' } },
      bottom: { style: 'hair', color: { argb: 'D9D9D9' } },
      right: { style: 'hair', color: { argb: 'D9D9D9' } },
    };

    const headerFill = {
      type: 'pattern' as const,
      pattern: 'solid' as const,
      fgColor: { argb: 'F8F9FB' },
    };

    const monthFill = {
      type: 'pattern' as const,
      pattern: 'solid' as const,
      fgColor: { argb: 'EEF3F7' },
    };

    const totalFill = {
      type: 'pattern' as const,
      pattern: 'solid' as const,
      fgColor: { argb: 'EEF4FF' },
    };

    // Header row 1
    ws.mergeCells(1, 1, 2, 1);
    const staffHeaderCell = ws.getCell(1, 1);
    staffHeaderCell.value = 'STAFF';
    staffHeaderCell.font = { bold: true };
    staffHeaderCell.alignment = { vertical: 'middle', horizontal: 'left' };
    staffHeaderCell.fill = headerFill;
    staffHeaderCell.border = faintBorder;

    for (let i = 0; i < input.monthKeys.length; i++) {
      const startCol = 2 + i * 2;
      const endCol = startCol + 1;

      ws.mergeCells(1, startCol, 1, endCol);

      const monthCell = ws.getCell(1, startCol);
      monthCell.value = input.monthLabels[i];
      monthCell.font = { bold: true };
      monthCell.alignment = { vertical: 'middle', horizontal: 'center' };
      monthCell.fill = monthFill;
      monthCell.border = faintBorder;

      const dateHead = ws.getCell(2, startCol);
      dateHead.value = 'DATE';
      dateHead.font = { bold: true, size: 10 };
      dateHead.alignment = { vertical: 'middle', horizontal: 'center' };
      dateHead.fill = headerFill;
      dateHead.border = faintBorder;

      const salaryHead = ws.getCell(2, endCol);
      salaryHead.value = 'SALARY';
      salaryHead.font = { bold: true, size: 10 };
      salaryHead.alignment = { vertical: 'middle', horizontal: 'center' };
      salaryHead.fill = headerFill;
      salaryHead.border = faintBorder;
    }

    ws.getRow(1).height = 22;
    ws.getRow(2).height = 20;

    let currentRow = 3;

    for (const item of input.data) {
      const rowCount = getStaffSalaryMaxRows(item, input.monthKeys);
      const startRow = currentRow;
      const endRow = currentRow + rowCount - 1;

      if (rowCount > 1) {
        ws.mergeCells(startRow, 1, endRow, 1);
      }

      const staffCell = ws.getCell(startRow, 1);
      staffCell.value = item.staffName ?? '';
      staffCell.font = { bold: true };
      staffCell.alignment = { vertical: 'top', horizontal: 'left' };
      staffCell.border = faintBorder;

      for (let r = 0; r < rowCount; r++) {
        const excelRow = ws.getRow(currentRow + r);

        for (let m = 0; m < input.monthKeys.length; m++) {
          const monthKey = input.monthKeys[m];
          const details = getStaffSalaryMonthDetails(item, monthKey);
          const detail = details[r] ?? null;

          const dateCol = 2 + m * 2;
          const salaryCol = dateCol + 1;

          const dateCell = excelRow.getCell(dateCol);
          const salaryCell = excelRow.getCell(salaryCol);

          dateCell.value = detail?.salaryDate ? this.formatDateText(detail.salaryDate) : '-';
          salaryCell.value = detail?.salaryAmount ?? '-';

          dateCell.alignment = { vertical: 'middle', horizontal: 'left' };
          salaryCell.alignment = {
            vertical: 'middle',
            horizontal: detail?.salaryAmount != null ? 'right' : 'center',
          };

          dateCell.border = faintBorder;
          salaryCell.border = faintBorder;

          if (typeof salaryCell.value === 'number') {
            salaryCell.numFmt = '#,##0';
          }
        }
      }

      currentRow += rowCount;
    }

    // Final total row
    const totalRow = ws.getRow(currentRow);
    const totalTitleCell = totalRow.getCell(1);
    totalTitleCell.value = 'Total';
    totalTitleCell.font = { bold: true, color: { argb: '1D4ED8' } };
    totalTitleCell.fill = totalFill;
    totalTitleCell.border = faintBorder;
    totalTitleCell.alignment = { vertical: 'middle', horizontal: 'left' };

    for (let m = 0; m < input.monthKeys.length; m++) {
      const monthKey = input.monthKeys[m];
      const dateCol = 2 + m * 2;
      const salaryCol = dateCol + 1;

      const labelCell = totalRow.getCell(dateCol);
      const valueCell = totalRow.getCell(salaryCol);

      labelCell.value = 'Total';
      valueCell.value = getStaffSalaryOverallMonthTotal(input.data, monthKey);

      labelCell.font = { bold: true, color: { argb: '1D4ED8' } };
      valueCell.font = { bold: true, color: { argb: '1D4ED8' } };

      labelCell.fill = totalFill;
      valueCell.fill = totalFill;

      labelCell.border = faintBorder;
      valueCell.border = faintBorder;

      labelCell.alignment = { vertical: 'middle', horizontal: 'left' };
      valueCell.alignment = { vertical: 'middle', horizontal: 'right' };
      valueCell.numFmt = '#,##0';
    }

    // Apply borders/fill for merged header cells too
    for (let col = 1; col <= totalColumns; col++) {
      ws.getCell(2, col).border = faintBorder;
      if (col !== 1) {
        ws.getCell(1, col).border = faintBorder;
      }
    }

    ws.views = [{ state: 'frozen', xSplit: 1, ySplit: 2 }];

    return wb;
  }

  private static formatDateText(value: string | Date): string {
    if (typeof value === 'string') {
      const match = value.match(/^(\d{4})-(\d{2})-(\d{2})/);
      if (match) {
        return `${match[3]}-${match[2]}-${match[1]}`;
      }
    }

    const d = new Date(value);
    if (isNaN(d.getTime())) {
      return '-';
    }

    const dd = String(d.getDate()).padStart(2, '0');
    const mm = String(d.getMonth() + 1).padStart(2, '0');
    const yyyy = d.getFullYear();
    return `${dd}-${mm}-${yyyy}`;
  }
}
