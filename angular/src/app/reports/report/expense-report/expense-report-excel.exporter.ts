import * as ExcelJS from 'exceljs/dist/exceljs.min.js';
import {
  ExpenseReportDto,
  ExpenseReportDetailDto,
} from 'src/app/proxy/reports/expense-report';
import {
  ExpenseReportExcelExportInput,
  getExpenseMaxRows,
  getExpenseMonthDetails,
  getExpenseOverallMonthTotal,
} from './expense-report.helpers';

export class ExpenseReportExcelJsExporter {
  static async buildWorkbook(
    input: ExpenseReportExcelExportInput
  ): Promise<ExcelJS.Workbook> {
    const wb = new ExcelJS.Workbook();
    const ws = wb.addWorksheet(input.sheetName);

    const monthBlockSize = 5;
    const firstColWidth = 24;

    const faintSide = {
      style: 'thin' as const,
      color: { argb: 'FFD9DEE7' },
    };

    const faintBorder: ExcelJS.Borders = {
      top: faintSide,
      left: faintSide,
      bottom: faintSide,
      right: faintSide,
    };

    const monthSeparatorSide = {
      style: 'medium' as const,
      color: { argb: 'FFC4D1DF' },
    };

    const applyBorder = (cell: ExcelJS.Cell, isMonthStart = false) => {
      cell.border = {
        top: faintSide,
        right: faintSide,
        bottom: faintSide,
        left: isMonthStart ? monthSeparatorSide : faintSide,
      };
    };

    const applyRowBorders = (row: ExcelJS.Row) => {
      row.eachCell({ includeEmpty: true }, cell => {
        cell.border = faintBorder;
      });
    };

    const monthStartColIndexes = input.monthKeys.map(
      (_, i) => 2 + i * monthBlockSize
    );

    const isMonthStartCol = (col: number) => monthStartColIndexes.includes(col);

    const totalCols = 1 + input.monthKeys.length * monthBlockSize;

    // Header row 1
    const headerRow1 = ws.getRow(1);
    headerRow1.getCell(1).value = 'Expense Category';
    headerRow1.getCell(1).alignment = { vertical: 'middle', horizontal: 'left' };
    headerRow1.getCell(1).font = { bold: true };
    headerRow1.getCell(1).fill = {
      type: 'pattern',
      pattern: 'solid',
      fgColor: { argb: 'FFF8F9FB' },
    };

    ws.mergeCells(1, 1, 2, 1);

    input.monthLabels.forEach((label, index) => {
      const startCol = 2 + index * monthBlockSize;
      const endCol = startCol + monthBlockSize - 1;

      ws.mergeCells(1, startCol, 1, endCol);
      const cell = headerRow1.getCell(startCol);
      cell.value = label;
      cell.alignment = { vertical: 'middle', horizontal: 'center' };
      cell.font = { bold: true };
      cell.fill = {
        type: 'pattern',
        pattern: 'solid',
        fgColor: { argb: 'FFEEF3F7' },
      };
    });

    // Header row 2
    const headerRow2 = ws.getRow(2);
    input.monthKeys.forEach((_, index) => {
      const startCol = 2 + index * monthBlockSize;

      headerRow2.getCell(startCol).value = 'Date';
      headerRow2.getCell(startCol + 1).value = 'Title';
      headerRow2.getCell(startCol + 2).value = 'Paid To';
      headerRow2.getCell(startCol + 3).value = 'Remarks';
      headerRow2.getCell(startCol + 4).value = 'Amount';

      for (let c = startCol; c <= startCol + 4; c++) {
        const cell = headerRow2.getCell(c);
        cell.font = { bold: true };
        cell.alignment = {
          vertical: 'middle',
          horizontal: c === startCol + 4 ? 'right' : 'center',
        };
        cell.fill = {
          type: 'pattern',
          pattern: 'solid',
          fgColor: { argb: 'FFF8F9FB' },
        };
      }
    });

    // Apply header borders
    for (let r = 1; r <= 2; r++) {
      const row = ws.getRow(r);
      for (let c = 1; c <= totalCols; c++) {
        applyBorder(row.getCell(c), isMonthStartCol(c));
      }
    }

    let currentRowIndex = 3;

    for (const item of input.data ?? []) {
      const maxRows = getExpenseMaxRows(item, input.monthKeys);

      const startRowIndex = currentRowIndex;
      const endRowIndex = currentRowIndex + maxRows - 1;

      if (maxRows > 1) {
        ws.mergeCells(startRowIndex, 1, endRowIndex, 1);
      }

      const categoryCell = ws.getCell(startRowIndex, 1);
      categoryCell.value = item.expenseCategoryName ?? '';
      categoryCell.font = { bold: true };
      categoryCell.alignment = { vertical: 'top', horizontal: 'left' };

      for (let rowOffset = 0; rowOffset < maxRows; rowOffset++) {
        const excelRow = ws.getRow(currentRowIndex + rowOffset);

        input.monthKeys.forEach((mk, monthIndex) => {
          const details = getExpenseMonthDetails(item, mk);
          const detail = details[rowOffset] as ExpenseReportDetailDto | undefined;

          const startCol = 2 + monthIndex * monthBlockSize;

          excelRow.getCell(startCol).value = detail?.expenseDate
            ? formatDate(detail.expenseDate as any)
            : '-';
          excelRow.getCell(startCol + 1).value = detail?.title ?? '-';
          excelRow.getCell(startCol + 2).value = detail?.paidTo ?? '-';
          excelRow.getCell(startCol + 3).value = detail?.remarks ?? '-';
          excelRow.getCell(startCol + 4).value =
            detail?.amount != null ? detail.amount : '-';

          excelRow.getCell(startCol + 4).alignment = {
            vertical: 'middle',
            horizontal: 'right',
          };
        });

        for (let c = 1; c <= totalCols; c++) {
          applyBorder(excelRow.getCell(c), isMonthStartCol(c));
        }

        currentRowIndex++;
      }
    }

    // Final total row
    const totalRow = ws.getRow(currentRowIndex);
    totalRow.getCell(1).value = 'Total';
    totalRow.getCell(1).font = { bold: true };
    totalRow.getCell(1).fill = {
      type: 'pattern',
      pattern: 'solid',
      fgColor: { argb: 'FFEEF4FF' },
    };

    input.monthKeys.forEach((mk, monthIndex) => {
      const startCol = 2 + monthIndex * monthBlockSize;

      ws.mergeCells(currentRowIndex, startCol, currentRowIndex, startCol + 3);

      const labelCell = totalRow.getCell(startCol);
      labelCell.value = 'Total';
      labelCell.font = { bold: true, color: { argb: 'FF1D4ED8' } };
      labelCell.fill = {
        type: 'pattern',
        pattern: 'solid',
        fgColor: { argb: 'FFEEF4FF' },
      };
      labelCell.alignment = { vertical: 'middle', horizontal: 'left' };

      const valueCell = totalRow.getCell(startCol + 4);
      valueCell.value = getExpenseOverallMonthTotal(input.data, mk);
      valueCell.font = { bold: true, color: { argb: 'FF1D4ED8' } };
      valueCell.fill = {
        type: 'pattern',
        pattern: 'solid',
        fgColor: { argb: 'FFEEF4FF' },
      };
      valueCell.alignment = { vertical: 'middle', horizontal: 'right' };
      valueCell.numFmt = '#,##0';
    });

    for (let c = 1; c <= totalCols; c++) {
      applyBorder(totalRow.getCell(c), isMonthStartCol(c));
    }

    // Column widths
    ws.getColumn(1).width = firstColWidth;

    input.monthKeys.forEach((_, index) => {
      const startCol = 2 + index * monthBlockSize;
      ws.getColumn(startCol).width = 14;     // Date
      ws.getColumn(startCol + 1).width = 20; // Title
      ws.getColumn(startCol + 2).width = 16; // Paid To
      ws.getColumn(startCol + 3).width = 22; // Remarks
      ws.getColumn(startCol + 4).width = 14; // Amount
    });

    return wb;
  }
}

function formatDate(value: string | Date): string {
  const d = value instanceof Date ? value : new Date(value);
  const dd = `${d.getDate()}`.padStart(2, '0');
  const mm = `${d.getMonth() + 1}`.padStart(2, '0');
  const yyyy = d.getFullYear();
  return `${dd}-${mm}-${yyyy}`;
}