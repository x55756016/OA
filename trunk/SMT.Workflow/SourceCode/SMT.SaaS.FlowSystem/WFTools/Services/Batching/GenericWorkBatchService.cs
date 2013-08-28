using System;
using System.Transactions;
using System.Workflow.Runtime.Hosting;
using WFTools.Utilities.Diagnostics;

namespace WFTools.Services.Batching
{
    /// <summary>
    /// Abstract implementation of the <see cref="WorkflowCommitWorkBatchService"/ >
    /// that provides methods for doing something with the transaction at various
    /// point in the work batch's lifecycle.
    /// </summary>
    public abstract class GenericWorkBatchService : WorkflowCommitWorkBatchService
    {
        /// <summary>
        /// Called just after a transaction is created for this workbatch.
        /// </summary>
        /// <param name="transaction">
        /// The transaction that was created.
        /// </param>
        protected abstract void TransactionCreated(Transaction transaction);

        /// <summary>
        /// Called after the workbatch is committed.
        /// </summary>
        protected abstract void WorkBatchCommitted(Transaction transaction);

        /// <summary>
        /// Called after the workbatch has rolled back.
        /// </summary>
        protected abstract void WorkBatchRolledback(Transaction transaction);

        protected override void CommitWorkBatch(CommitWorkBatchCallback commitWorkBatchCallback)
        {
            TraceHelper.Trace();

            Transaction transactionToUse;
            if (Transaction.Current == null)
            {
                transactionToUse = new CommittableTransaction();
                WfLogHelper.WriteLog("CommitWorkBatch提交TransactionScope事务Transaction.Current==null");
            }
            else
            {
                transactionToUse = Transaction.Current.DependentClone(DependentCloneOption.BlockCommitUntilComplete);
                WfLogHelper.WriteLog("CommitWorkBatch提交TransactionScope事务Transaction.Current!=null" );
            }

            TransactionCreated(transactionToUse);

            try
            {
                using (TransactionScope txScope = new TransactionScope(transactionToUse))
                {
                    commitWorkBatchCallback();
                    txScope.Complete();
                    WfLogHelper.WriteLog("CommitWorkBatch提交TransactionScope事务Complete完成......");
                }

                CommittableTransaction committableTransaction = transactionToUse as CommittableTransaction;
                if (committableTransaction != null)
                {
                    committableTransaction.Commit();
                    WfLogHelper.WriteLog("CommitWorkBatch提交committableTransaction事务Complete完成......");
                }

                DependentTransaction dependentTransaction = transactionToUse as DependentTransaction;
                if (dependentTransaction != null)
                {
                    dependentTransaction.Complete();
                    WfLogHelper.WriteLog("CommitWorkBatch提交dependentTransaction事务Complete完成......");
                }

                WorkBatchCommitted(transactionToUse);
            }
            catch (Exception e)
            {
                transactionToUse.Rollback(e);
                
                WorkBatchRolledback(transactionToUse);

                throw;
            }
            finally
            {
                if (transactionToUse != null)
                    transactionToUse.Dispose();
            }
        }
    }
}
