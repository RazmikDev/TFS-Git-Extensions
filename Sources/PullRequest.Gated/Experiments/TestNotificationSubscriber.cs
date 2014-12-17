using System;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.TeamFoundation.Common;
using Microsoft.TeamFoundation.Framework.Server;
using Microsoft.TeamFoundation.Git.Server;

namespace TeamFoundation.Git.Server.Builds.PullRequest.Gated.Experiments
{
	public class TestNotificationSubscriber : ISubscriber
	{
		#region Implementation of ISubscriber

		public Type[] SubscribedTypes()
		{
			return new[]
			{
				typeof(PushNotification),
				typeof(PullRequestNotification),
				typeof(PullRequestCreatedNotification)
			};
		}

		public EventNotificationStatus ProcessEvent(TeamFoundationRequestContext requestContext, NotificationType notificationType, object notificationEventArgs, out int statusCode, out string statusMessage, out ExceptionPropertyCollection properties)
		{
			properties = null;
			statusCode = 0;
			statusMessage = null;

			if (notificationType != NotificationType.DecisionPoint)
				return EventNotificationStatus.ActionApproved;

			var notification = notificationEventArgs as PullRequestNotification;
			if (notification != null)
			{
				var gitPullRequestService = requestContext.GetService<TeamFoundationGitPullRequestService>();
				var gitrepositoryService = requestContext.GetService<TeamFoundationGitRepositoryService>();
				var pullRequest = gitPullRequestService.GetPullRequestDetails(requestContext, notification.PullRequestId);
				var repository = gitrepositoryService.FindRepositoryById(requestContext, pullRequest.RepositoryId);

			
				var reviews = pullRequest.Reviewers.ToList();
				reviews.Add(new TfsGitPullRequest.ReviewerWithVote(new Guid(), 4, ReviewerVoteStatus.Rejected));
				gitPullRequestService.UpdatePullRequest(requestContext, repository, pullRequest.PullRequestId, pullRequest.Status, pullRequest.Title, pullRequest.Description, reviews);

				var messageBuilder = new StringBuilder();
				messageBuilder.AppendFormat(DateTime.Now.ToShortTimeString());
				messageBuilder.Append(" : ");

				messageBuilder.AppendFormat(@"PullRequestId = {0}", pullRequest.PullRequestId);
				messageBuilder.AppendLine();

				messageBuilder.AppendFormat(@"MergeId = {0}", pullRequest.MergeId);
				messageBuilder.AppendLine();

				messageBuilder.AppendFormat(@"PullRequestId = {0}", pullRequest.SourceBranchName);
				messageBuilder.AppendLine();

				messageBuilder.AppendFormat(@"SourceBranchName = {0}", pullRequest.TargetBranchName);
				messageBuilder.AppendLine();

				messageBuilder.AppendFormat(@"LastMergeCommit = {0}", pullRequest.LastMergeCommit);
				messageBuilder.AppendLine();

				messageBuilder.AppendFormat(@"LastMergeSourceCommit = {0}", pullRequest.LastMergeSourceCommit);
				messageBuilder.AppendLine();

				messageBuilder.AppendFormat(@"LastMergeTargetCommit = {0}", pullRequest.LastMergeTargetCommit);
				messageBuilder.AppendLine();

				messageBuilder.AppendFormat(@"Status = {0}", pullRequest.Status);
				messageBuilder.AppendLine();

				messageBuilder.AppendFormat(@"Title = {0}", pullRequest.Title);
				messageBuilder.AppendLine();

				messageBuilder.AppendFormat(@"MergeStatus = {0}", pullRequest.MergeStatus);
				messageBuilder.AppendLine();

				messageBuilder.AppendFormat(@"CompleteWhenMergedAuthority = {0}", pullRequest.CompleteWhenMergedAuthority);
				messageBuilder.AppendLine();

				messageBuilder.AppendFormat(@"Description = {0}", pullRequest.Description);
				messageBuilder.AppendLine();

				messageBuilder.AppendFormat(@"Creator = {0}", pullRequest.Creator);
				messageBuilder.AppendLine();

				messageBuilder.AppendFormat(@"CreationDate = {0}", pullRequest.CreationDate);
				messageBuilder.AppendLine();
				messageBuilder.AppendLine("==========================================================");


				File.AppendAllText(@"C:\TMP\NPTV_TEST_LOG.txt", messageBuilder.ToString());
			}

			return EventNotificationStatus.ActionApproved;
		}

		public string Name
		{
			get { return "NPTV TEST HANDLER"; }
		}

		public SubscriberPriority Priority
		{
			get { return SubscriberPriority.Normal; }
		}

		#endregion
	}
}