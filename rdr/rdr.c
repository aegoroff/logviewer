/*!
 * \brief   The file contains Hash LINQ implementation
 * \author  \verbatim
            Created by: Alexander Egorov
            \endverbatim
 * \date    \verbatim
            Creation date: 2011-11-14
            \endverbatim
 * Copyright: (c) Alexander Egorov 2009-2013
 */

#include "targetver.h"
#include <locale.h>
#include "rdr.h"
#include "argtable2.h"
#include "apr.h"
#include "apr_pools.h"
#include "apr_strings.h"

#ifdef WIN32
#include "..\srclib\DebugHelplers.h"
#endif

#define ERROR_BUFFER_SIZE 2 * BINARY_THOUSAND
#define LINE_FEED '\n'

#define PATH_ELT_SEPARATOR '\\'
#define NUMBER_PARAM_FMT_STRING "%lu"
#define BIG_NUMBER_PARAM_FMT_STRING "%llu"

#define MAX_LINE_SIZE 32 * BINARY_THOUSAND - 1


void PrintError(apr_status_t status)
{
    char errbuf[ERROR_BUFFER_SIZE];
    apr_strerror(status, errbuf, ERROR_BUFFER_SIZE);
    CrtPrintf("%s", errbuf); //-V111
    NewLine();
}

int main(int argc, const char* const argv[])
{
    apr_pool_t* pool = NULL;
    apr_status_t status = APR_SUCCESS;
    int nerrors;
    apr_off_t limitValue = 0;
    apr_off_t offsetValue = 0;

    struct arg_file *file          = arg_file0("f", "file", NULL, "full path file to calculate hash sum for");
    struct arg_lit  *help          = arg_lit0("h", "help", "print this help and exit");
    struct arg_end  *end           = arg_end(10);

    void* argtable[] = { file, help, end };

#ifdef WIN32
#ifndef _DEBUG  // only Release configuration dump generating
    SetUnhandledExceptionFilter(TopLevelFilter);
#endif
#endif

    setlocale(LC_ALL, ".ACP");
    setlocale(LC_NUMERIC, "C");

    status = apr_app_initialize(&argc, &argv, NULL);
    if (status != APR_SUCCESS) {
        CrtPrintf("Couldn't initialize APR");
        NewLine();
        PrintError(status);
        return EXIT_FAILURE;
    }
    atexit(apr_terminate);
    apr_pool_create(&pool, NULL);

    if (arg_nullcheck(argtable) != 0) {
        PrintSyntax(argtable);
        goto cleanup;
    }

    /* Parse the command line as defined by argtable[] */
    nerrors = arg_parse(argc, argv, argtable);

    if (help->count > 0) {
        PrintSyntax(argtable);
        goto cleanup;
    }
    if (nerrors > 0 || argc < 2) {
        arg_print_errors(stdout, end, PROGRAM_NAME);
        PrintSyntax(argtable);
        goto cleanup;
    }



cleanup:
    /* deallocate each non-null entry in argtable[] */
    arg_freetable(argtable, sizeof(argtable) / sizeof(argtable[0]));
    apr_pool_destroy(pool);
    return EXIT_SUCCESS;
}

void PrintSyntax(void* argtable) {
    PrintCopyright();
    arg_print_syntax(stdout, argtable, NEW_LINE NEW_LINE);
    arg_print_glossary_gnu(stdout,argtable);
}

void PrintCopyright(void)
{
    CrtPrintf(COPYRIGHT_FMT, APP_NAME);
}
